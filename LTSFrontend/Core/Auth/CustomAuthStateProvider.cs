using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using LTSFrontend.State;

namespace LTSFrontend.Core.Auth
{
    /// <summary>
    /// Bridges our JWT-based session (UserSessionState) with Blazor's
    /// [Authorize] / <AuthorizeView> / <CascadingAuthenticationState>
    /// infrastructure, so the rest of the app can use them normally.
    /// </summary>
    public class CustomAuthStateProvider(UserSessionState _session, ITokenStorageService _tokenStorage) : AuthenticationStateProvider
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
                if (stored != null && stored.AccessTokenExpiry > DateTime.UtcNow)
                {
                    _session.Set(stored.UserID, stored.FullName, stored.Email, stored.Role,stored.AccessToken, stored.AccessTokenExpiry);
                }
            }

            if (!_session.IsAuthenticated)
            {
                return Anonymous;
            }

            var identity = BuildIdentity(_session);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        /// <summary>Call right after a successful login/register+verify.</summary>
        public async Task MarkUserAsAuthenticatedAsync(int userId, string fullName, string email, string? role,string accessToken, DateTime accessTokenExpiry)
        {
            _session.Set(userId, fullName, email, role, accessToken, accessTokenExpiry);
            await _tokenStorage.SaveSessionAsync(new StoredSession(userId, fullName, email, role, accessToken, accessTokenExpiry));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(BuildIdentity(_session)))));
        }

        /// <summary>Call after logout (or when the server rejects the token).</summary>
        public async Task MarkUserAsLoggedOutAsync()
        {
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
