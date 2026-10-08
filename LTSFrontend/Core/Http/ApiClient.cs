using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LTSFrontend.Core.Auth;
using LTSFrontend.Core.Exceptions;
using LTSFrontend.Core.DTOs;
using LTSFrontend.State;

namespace LTSFrontend.Core.Http
{
    public class ApiClient : IDisposable
    {
        public HttpClient Http { get; }

        private readonly UserSessionState _session;
        private readonly ITokenStorageService _tokenStorage;
        private readonly CookieContainer _cookieContainer;
        private readonly ILogger<ApiClient>? _logger;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly TokenRefreshGate _refreshGate;

        public ApiClient(HttpClient httpClient, UserSessionState session, ITokenStorageService tokenStorage, TokenRefreshGate refreshGate, CookieContainer cookieContainer, ILogger<ApiClient>? logger = null)
        {
            Http = httpClient;
            _session = session;
            _tokenStorage = tokenStorage;
            _refreshGate = refreshGate;
            _cookieContainer = cookieContainer;
            _logger = logger;
        }

        public string? GetCurrentRefreshToken()
        {
            if (Http.BaseAddress == null)
            {
                return null;
            }

            return _cookieContainer.GetCookies(Http.BaseAddress)["refreshToken"]?.Value;
        }

        public void SeedRefreshTokenCookie(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken) || Http.BaseAddress == null)
            {
                return;
            }

             _cookieContainer.Add(Http.BaseAddress, new Cookie("refreshToken", refreshToken));
        }

        public void Dispose()
        {
            Http.Dispose();
        }

        public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
        {
            return await SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url), ct);
        }

        public async Task<T?> PostAsync<T>(string url, object? body = null, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (body != null)
            {
                request.Content = JsonContent.Create(body, options: JsonOptions);
            }

            return await SendAsync<T>(request, ct);
        }

        public async Task<T?> PutAsync<T>(string url, object? body = null, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            if (body != null)
            {
                request.Content = JsonContent.Create(body, options: JsonOptions);
            }

            return await SendAsync<T>(request, ct);
        }

        public async Task<T?> PostFormAsync<T>(string url, MultipartFormDataContent form, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
            return await SendAsync<T>(request, ct);
        }

        public async Task<T?> PutFormAsync<T>(string url, MultipartFormDataContent form, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = form };
            return await SendAsync<T>(request, ct);
        }

        public async Task<T?> DeleteAsync<T>(string url, CancellationToken ct = default)
        {
            return await SendAsync<T>(new HttpRequestMessage(HttpMethod.Delete, url), ct);
        }

        public async Task<HttpResponseMessage> GetRawAsync(string url, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await EnsureAuthorizationHeaderAsync(request);
            var sentToken = request.Headers.Authorization?.Parameter;
            var response = await Http.SendAsync(request, ct);

            // Same one-time "refresh and retry" as SendAsync, for file downloads.
            if (response.StatusCode == HttpStatusCode.Unauthorized && _session.UserID != 0
                && await TryRefreshAccessTokenAsync(sentToken) == RefreshResult.Refreshed)
            {
                response.Dispose();
                var retry = new HttpRequestMessage(HttpMethod.Get, url);
                retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
                response = await Http.SendAsync(retry, ct);
            }

            return response;
        }

        private enum RefreshResult
        {
            /// <summary>A usable access token is now in the session.</summary>
            Refreshed,
            /// <summary>The backend definitively refused the refresh token (expired / revoked / missing) - the session is over.</summary>
            Rejected,
            /// <summary>Temporary problem (rate limited, backend down, network) - the session may still be fine, try again later.</summary>
            Failed
        }

        private async Task EnsureAuthorizationHeaderAsync(HttpRequestMessage request)
        {
            if (!_session.IsAuthenticated)
            {
                var stored = await _tokenStorage.GetSessionAsync();
                if (stored != null)
                {
                    _session.Set(stored.UserID, stored.FullName, stored.Email, stored.Role, stored.AccessToken, stored.AccessTokenExpiry);
                    SeedRefreshTokenCookie(stored.RefreshToken);
                }
            }

            bool hasKnownIdentity = _session.UserID != 0;
            bool tokenMissingOrExpiring = string.IsNullOrWhiteSpace(_session.AccessToken) || !_session.AccessTokenExpiry.HasValue || _session.AccessTokenExpiry.Value <= DateTime.UtcNow.AddSeconds(30);

            if (hasKnownIdentity && tokenMissingOrExpiring)
            {
                await TryRefreshAccessTokenAsync();
            }

            if (!string.IsNullOrWhiteSpace(_session.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
            }
        }

        /// <param name="rejectedToken">
        /// Set when the backend answered 401 to a request sent with this access token. In that case the
        /// "token still looks valid" shortcut must NOT be trusted (the clock/expiry we hold can be stale),
        /// unless another request already swapped in a newer token while we waited for the lock.
        /// </param>
        private async Task<RefreshResult> TryRefreshAccessTokenAsync(string? rejectedToken = null)
        {
            await _refreshGate.Lock.WaitAsync();
            try
            {
                bool looksValid = !string.IsNullOrWhiteSpace(_session.AccessToken) && _session.AccessTokenExpiry.HasValue && _session.AccessTokenExpiry.Value > DateTime.UtcNow.AddSeconds(30);

                if (rejectedToken == null && looksValid)
                {
                    return RefreshResult.Refreshed;
                }

                // Another request already refreshed while we waited - just use its new token.
                if (rejectedToken != null && !string.IsNullOrWhiteSpace(_session.AccessToken) && _session.AccessToken != rejectedToken)
                {
                    return RefreshResult.Refreshed;
                }

                 _logger?.LogInformation("[ApiClient] Silent refresh attempt. Cookie jar has refreshToken={HasRt}", !string.IsNullOrWhiteSpace(GetCurrentRefreshToken()));
                var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.Auth.RefreshToken);
                var response = await Http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogWarning("[ApiClient] Silent refresh REJECTED by backend: HTTP {Status}. Body: {Body}", (int)response.StatusCode, await response.Content.ReadAsStringAsync());

                    // 400/401/403 = the refresh token itself is bad (expired/revoked/missing): the session is over.
                    // Anything else (429 rate limit, 5xx, ...) is temporary and must NOT end the session.
                    return response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                        ? RefreshResult.Rejected
                        : RefreshResult.Failed;
                }

                var raw = await response.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<ApiResponse<Features.Auth.DTOs.RefreshTokenResponseDTO>>(raw, JsonOptions);

                if (parsed?.Success != true || parsed.Data == null)
                {
                    return RefreshResult.Failed;
                }

                _session.UpdateAccessToken(parsed.Data.AccessToken, parsed.Data.AccessTokenExpiry);

                await _tokenStorage.SaveSessionAsync(new StoredSession(_session.UserID, _session.FullName, _session.Email, _session.Role, _session.AccessToken!, _session.AccessTokenExpiry!.Value, GetCurrentRefreshToken()));
                _logger?.LogInformation("[ApiClient] Silent refresh SUCCEEDED; new access token expires {Expiry:o}", _session.AccessTokenExpiry);

                return RefreshResult.Refreshed;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[ApiClient] Silent refresh threw an exception.");
                return RefreshResult.Failed;
            }
            finally
            {
                _refreshGate.Lock.Release();
            }
        }

        private async Task EndExpiredSessionAsync()
        {
            _logger?.LogWarning("[ApiClient] Refresh token rejected - clearing the local session.");
            _session.Clear();
            await _tokenStorage.ClearSessionAsync();
        }

        /// <summary>
        /// Builds a fresh, re-sendable copy of a request (null when its body cannot be replayed, e.g. a
        /// multipart file upload - those simply surface the 401 as before). Bodies are buffered to bytes first.
        /// </summary>
        private static async Task<HttpRequestMessage?> TryCreateRetryCopyAsync(HttpRequestMessage original)
        {
            var copy = new HttpRequestMessage(original.Method, original.RequestUri);

            if (original.Content == null)
                return copy;

            if (original.Content is MultipartContent)
                return null;

            var bytes = await original.Content.ReadAsByteArrayAsync();
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = original.Content.Headers.ContentType;
            copy.Content = content;
            return copy;
        }

        private async Task<T?> SendAsync<T>(HttpRequestMessage request, CancellationToken ct)
        {
            await EnsureAuthorizationHeaderAsync(request);
            var sentToken = request.Headers.Authorization?.Parameter;

            // An HttpRequestMessage can only be sent once, so keep a copy to replay after a token refresh.
            var retryCopy = await TryCreateRetryCopyAsync(request);

            HttpResponseMessage response;
            try
            {
                response = await Http.SendAsync(request, ct);

                // The access token we held was rejected (expired, or our cached expiry was stale). Instead of
                // failing the user's action, refresh once and replay the request with the new token.
                if (response.StatusCode == HttpStatusCode.Unauthorized && _session.UserID != 0)
                {
                    var refresh = await TryRefreshAccessTokenAsync(sentToken);

                    if (refresh == RefreshResult.Refreshed && retryCopy != null)
                    {
                        response.Dispose();
                        retryCopy.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
                        response = await Http.SendAsync(retryCopy, ct);
                    }
                    else if (refresh == RefreshResult.Rejected)
                    {
                        // Refresh token is dead too: end the session cleanly so the app sends the user to login
                        // instead of leaving every screen failing with "401 Unauthorized".
                        await EndExpiredSessionAsync();
                        throw new ApiException("Your session has expired. Please sign in again.", (int)HttpStatusCode.Unauthorized);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Backend se connect nahi ho saka. Ensure LTSBackend API is running on " + Http.BaseAddress, null, new() { ex.Message });
            }

            var raw = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                string message = $"Request failed with status {(int)response.StatusCode} ({response.StatusCode}).";
                List<string>? errors = null;

                if (!string.IsNullOrWhiteSpace(raw))
                {
                    try
                    {
                        var problem = JsonSerializer.Deserialize<ApiErrorEnvelope>(raw, JsonOptions);
                        if (problem != null)
                        {
                            if (!string.IsNullOrWhiteSpace(problem.Message))
                            {
                                message = problem.Message;
                            }
                            errors = problem.Errors;
                        }
                    }
                    catch (JsonException)
                    {
                        // Not JSON at all (e.g. an IIS/Kestrel error page) -
                        // keep the generic status-code fallback message.
                    }
                }
                throw new ApiException(message, (int)response.StatusCode, errors);
            }

            ApiResponse<T>? parsed = null;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    parsed = JsonSerializer.Deserialize<ApiResponse<T>>(raw, JsonOptions);
                }
                catch (JsonException)
                {
                    parsed = null;
                }
            }

            // If expected return type is bool and response is successful with null/empty content
            if (typeof(T) == typeof(bool) && parsed == null)
            {
                return (T)(object)true;
            }

            if (parsed == null)
            {
                throw new ApiException("Server se invalid response mila.", (int)response.StatusCode);
            }

            if (!parsed.Success)
            {
                throw new ApiException(parsed.Message, (int)response.StatusCode, parsed.Errors);
            }

            return parsed.Data;
        }
    }
}
