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
            return await Http.SendAsync(request, ct);
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

        private async Task<bool> TryRefreshAccessTokenAsync()
        {
            await _refreshGate.Lock.WaitAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(_session.AccessToken) && _session.AccessTokenExpiry.HasValue && _session.AccessTokenExpiry.Value > DateTime.UtcNow.AddSeconds(30))
                {
                    return true;
                }

                 _logger?.LogInformation("[ApiClient] Silent refresh attempt. Cookie jar has refreshToken={HasRt}", !string.IsNullOrWhiteSpace(GetCurrentRefreshToken()));
                var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.Auth.RefreshToken);
                var response = await Http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogWarning("[ApiClient] Silent refresh REJECTED by backend: HTTP {Status}. Body: {Body}", (int)response.StatusCode, await response.Content.ReadAsStringAsync());
                    return false;
                }

                var raw = await response.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<ApiResponse<Features.Auth.DTOs.RefreshTokenResponseDTO>>(raw, JsonOptions);

                if (parsed?.Success != true || parsed.Data == null)
                {
                    return false;
                }

                _session.UpdateAccessToken(parsed.Data.AccessToken, parsed.Data.AccessTokenExpiry);

                await _tokenStorage.SaveSessionAsync(new StoredSession(_session.UserID, _session.FullName, _session.Email, _session.Role, _session.AccessToken!, _session.AccessTokenExpiry!.Value, GetCurrentRefreshToken()));
                _logger?.LogInformation("[ApiClient] Silent refresh SUCCEEDED; new access token expires {Expiry:o}", _session.AccessTokenExpiry);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[ApiClient] Silent refresh threw an exception.");
                return false;
            }
            finally
            {
                _refreshGate.Lock.Release();
            }
        }

        private async Task<T?> SendAsync<T>(HttpRequestMessage request, CancellationToken ct)
        {
            await EnsureAuthorizationHeaderAsync(request);
            HttpResponseMessage response;
            try
            {
                response = await Http.SendAsync(request, ct);
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
