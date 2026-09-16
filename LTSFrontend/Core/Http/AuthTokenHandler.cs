using System.Net.Http.Headers;
using LTSFrontend.Core.Auth;
using LTSFrontend.State;

namespace LTSFrontend.Core.Http
{
    public class AuthTokenHandler(UserSessionState _session, ITokenStorageService _tokenStorage) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            if (!_session.IsAuthenticated)
            {
                var stored = await _tokenStorage.GetSessionAsync();
                if (stored != null && stored.AccessTokenExpiry > DateTime.UtcNow)
                {
                    _session.Set(stored.UserID, stored.FullName, stored.Email, stored.Role,stored.AccessToken, stored.AccessTokenExpiry);
                }
            }

            if (!string.IsNullOrWhiteSpace(_session.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
