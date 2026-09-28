using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using LTSFrontend.Core.Http;
using LTSFrontend.State;

namespace LTSFrontend.Core.Auth
{
    /// <summary>
    /// Bridges our JWT-based session (UserSessionState) with Blazor's
    /// [Authorize] / <AuthorizeView> / <CascadingAuthenticationState>
    /// infrastructure, so the rest of the app can use them normally.
    /// </summary>
    public class CustomAuthStateProvider(UserSessionState _session, ITokenStorageService _tokenStorage, ApiClient _apiClient, ILogger<CustomAuthStateProvider> _logger) : AuthenticationStateProvider
    {
        private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!_session.IsAuthenticated)
            {
                // Try to silently restore the session from browser storage
                // (survives page refresh). Safe to call even during
                // prerender - TokenStorageService swallows JS interop errors.
                var stored = await _tokenStorage.GetSessionAsync();
                if (stored != null)
                {
                    _session.Set(stored.UserID, stored.FullName, stored.Email, stored.Role, stored.AccessToken, stored.AccessTokenExpiry);
                    _apiClient.SeedRefreshTokenCookie(stored.RefreshToken);
                    _logger.LogInformation(
                        "[AuthState] Restored session from storage for UserID={UserId}. IsAuthenticated now={IsAuth}",
                        stored.UserID, _session.IsAuthenticated);
                }
                else
                {
                    _logger.LogInformation("[AuthState] Nothing to restore from storage - staying Anonymous unless _session was already set some other way.");
                }
            }

            if (!_session.IsAuthenticated)
            {
                _logger.LogInformation("[AuthState] GetAuthenticationStateAsync returning ANONYMOUS.");
                return Anonymous;
            }

            var identity = BuildIdentity(_session);
            _logger.LogInformation("[AuthState] GetAuthenticationStateAsync returning AUTHENTICATED for UserID={UserId}.", _session.UserID);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        /// <summary>Call right after a successful login/register+verify.</summary>
        public async Task MarkUserAsAuthenticatedAsync(int userId, string fullName, string email, string? role,string accessToken, DateTime accessTokenExpiry)
        {
            _session.Set(userId, fullName, email, role, accessToken, accessTokenExpiry);
            var refreshToken = _apiClient.GetCurrentRefreshToken();
            _logger.LogInformation(
                "[AuthState] MarkUserAsAuthenticatedAsync for UserID={UserId}. GetCurrentRefreshToken() returned {HasRt} " +
                "(null/empty here means the login/verify call's Set-Cookie never reached this circuit's cookie jar - " +
                "check ApiClient's HttpClientHandler.UseCookies / CookieContainer wiring and the backend's Set-Cookie response).",
                userId, string.IsNullOrWhiteSpace(refreshToken) ? "NOTHING" : "a token");
            await _tokenStorage.SaveSessionAsync(new StoredSession(userId, fullName, email, role, accessToken, accessTokenExpiry, refreshToken));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(BuildIdentity(_session)))));
        }

        /// <summary>Call after logout (or when the server rejects the token).</summary>
        public async Task MarkUserAsLoggedOutAsync()
        {
            _logger.LogInformation("[AuthState] MarkUserAsLoggedOutAsync for UserID={UserId}.", _session.UserID);
            _session.Clear();
            await _tokenStorage.ClearSessionAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
        }

        private static ClaimsIdentity BuildIdentity(UserSessionState session)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, session.UserID.ToString()),
                new(ClaimTypes.Name, session.FullName),
                new(ClaimTypes.Email, session.Email)
            };

            if (!string.IsNullOrWhiteSpace(session.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, session.Role));
            }
            return new ClaimsIdentity(claims, authenticationType: "LTSAuth");
        }
    }
}
