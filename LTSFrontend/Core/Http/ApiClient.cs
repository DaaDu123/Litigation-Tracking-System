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

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly TokenRefreshGate _refreshGate;

        public ApiClient(HttpClient httpClient, UserSessionState session, ITokenStorageService tokenStorage, TokenRefreshGate refreshGate)
        {
            Http = httpClient;
            _session = session;
            _tokenStorage = tokenStorage;
            _refreshGate = refreshGate;
        }

        // SECURITY FIX: this app is Blazor Server (see Program.cs -
        // AddInteractiveServerComponents), so this HttpClient runs on the
        // SERVER, not in the user's browser. It used to be registered via
        // services.AddHttpClient<ApiClient>(...), which pools ONE shared
        // HttpMessageHandler (and therefore ONE shared CookieContainer,
        // since UseCookies=true) across every circuit/user on the server,
        // keyed only by the typed-client name - not per user. That meant:
        //   1. The refreshToken cookie set by one user's login could be
        //      overwritten by another concurrent user's login (both write
        //      to the SAME CookieContainer for the SAME domain+path+name),
        //      a real session/account-mixing risk.
        //   2. IHttpClientFactory recycles that shared handler by default
        //      every ~2 minutes, silently starting a brand-new EMPTY
        //      CookieContainer. Any user active longer than that lost
        //      their refreshToken cookie with no visible error - which is
        //      why Logout (and silent token refresh) started failing
        //      ("Refresh token not found in cookie") and LoginHistory's
        //      LogoutTime was never being saved.
        // Fix: ApiClient is now constructed once per DI scope (= once per
        // Blazor Server circuit = once per logged-in user - see
        // ServiceCollectionExtensions), each with its OWN dedicated
        // HttpClientHandler + CookieContainer that lives exactly as long
        // as that user's circuit and is disposed with it.
        public void Dispose()
        {
            Http.Dispose();
        }

        public Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
        {
            return SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url), ct);
        }

        public Task<T?> PostAsync<T>(string url, object? body = null, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (body != null)
            {
                request.Content = JsonContent.Create(body, options: JsonOptions);
            }

            return SendAsync<T>(request, ct);
        }

        public Task<T?> PutAsync<T>(string url, object? body = null, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            if (body != null)
            {
                request.Content = JsonContent.Create(body, options: JsonOptions);
            }

            return SendAsync<T>(request, ct);
        }

        public Task<T?> PostFormAsync<T>(string url, MultipartFormDataContent form, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
            return SendAsync<T>(request, ct);
        }

        public Task<T?> PutFormAsync<T>(string url, MultipartFormDataContent form, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = form };
            return SendAsync<T>(request, ct);
        }

        public Task<T?> DeleteAsync<T>(string url, CancellationToken ct = default)
        {
            return SendAsync<T>(new HttpRequestMessage(HttpMethod.Delete, url), ct);
        }

        // For endpoints that return raw bytes (file downloads) rather than
        // the ApiResponse<T> JSON envelope. Still runs the request through
        // EnsureAuthorizationHeaderAsync (attaches the Bearer token and
        // performs the same silent token refresh as every other call) -
        // calling Http.GetAsync directly, as DocumentService.DownloadAsync
        // used to, skips that step entirely and sends the request with NO
        // Authorization header, which is why downloads were failing for
        // every user regardless of their actual document permissions.
        public async Task<HttpResponseMessage> GetRawAsync(string url, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await EnsureAuthorizationHeaderAsync(request);
            return await Http.SendAsync(request, ct);
        }

        private async Task EnsureAuthorizationHeaderAsync(HttpRequestMessage request)
        {
            // If this circuit's session hasn't been populated yet (e.g. a
            // fresh browser tab / hard refresh, before CustomAuthStateProvider
            // got a chance to run), fall back to browser storage. By the
            // time an actual HTTP call is being made, the circuit is
            // guaranteed to be connected and JS interop guaranteed
            // available, so this is a safe, reliable last chance.
            if (!_session.IsAuthenticated)
            {
                var stored = await _tokenStorage.GetSessionAsync();
                if (stored != null && stored.AccessTokenExpiry > DateTime.UtcNow)
                {
                    _session.Set(stored.UserID, stored.FullName, stored.Email, stored.Role,stored.AccessToken, stored.AccessTokenExpiry);
                }
            }

            // SILENT TOKEN REFRESH: the access token is short-lived (7 min
            // by default - see JwtSettings.ExpiryMinutes on the backend).
            // Without this, once it expires every single request would
            // start failing with 401 until the user manually logs out and
            // back in, even though a perfectly valid refresh-token cookie
            // exists. Refresh proactively - a little before actual expiry -
            // using that HttpOnly cookie, so requests always go out with a
            // live token instead of reactively retrying after a 401 (which
            // would mean cloning/resending the original request, including
            // any multipart file-upload content that can only be read
            // once - proactive refresh avoids that whole class of bugs).
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
                // Someone else may have already refreshed while we were
                // waiting for the lock - re-check before making another
                // network call. This is now effective: _refreshGate is a
                // singleton, so "someone else" can be a completely
                // different ApiClient instance (different feature service)
                // and this check will still see the update, because
                // UserSessionState is also shared (Scoped, and Blazor WASM
                // has one root scope per app / browser tab).
                if (!string.IsNullOrWhiteSpace(_session.AccessToken) && _session.AccessTokenExpiry.HasValue && _session.AccessTokenExpiry.Value > DateTime.UtcNow.AddSeconds(30))
                {
                    return true;
                }

                // Talk to the raw HttpClient directly here, NOT this
                // class's own SendAsync<T> - that would recurse back into
                // EnsureAuthorizationHeaderAsync. The refresh-token cookie
                // (HttpOnly, sent automatically) is all this endpoint
                // needs; it's [AllowAnonymous] on the backend.
                var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.Auth.RefreshToken);
                var response = await Http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var raw = await response.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<ApiResponse<Features.Auth.DTOs.RefreshTokenResponseDTO>>(raw, JsonOptions);

                if (parsed?.Success != true || parsed.Data == null)
                {
                    return false;
                }

                _session.UpdateAccessToken(parsed.Data.AccessToken, parsed.Data.AccessTokenExpiry);

                // Persist the refreshed token too, so a brand new circuit
                // (new tab, F5) started right after this also picks up the
                // live token instead of the now-stale one that was
                // originally saved at login.
                await _tokenStorage.SaveSessionAsync(new StoredSession(_session.UserID, _session.FullName, _session.Email, _session.Role,_session.AccessToken!, _session.AccessTokenExpiry!.Value));

                return true;
            }
            catch
            {
                // Network hiccup, refresh token genuinely expired/revoked,
                // etc. - fall through and let the original request go out
                // with whatever token (possibly none) we already have; the
                // resulting 401, if any, surfaces normally to the caller.
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

            // ROOT-CAUSE FIX: error responses were being deserialized into
            // ApiResponse<T> - the SAME T the caller expects back on
            // success (e.g. `int` for CreateFirmAsync). LTSBackend's error
            // envelope never sets Data, so it comes back as JSON null/
            // absent; System.Text.Json refuses to bind that into a
            // non-nullable value-type T (int/long/bool), throws
            // JsonException, which the old code silently swallowed - along
            // with the perfectly good Message/Errors sitting right next to
            // that Data field in the same payload. That's why validation
            // errors (e.g. "Firm code can only contain letters, numbers,
            // and hyphens.") were showing up on screen as a generic
            // "Request failed with status 400" instead of the real reason.
            // Fix: parse error bodies with a small envelope that has no
            // Data property at all, so it can never fail to bind regardless
            // of what T the caller asked for. Only ever deserialize into
            // ApiResponse<T> once we know the call actually succeeded.
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
