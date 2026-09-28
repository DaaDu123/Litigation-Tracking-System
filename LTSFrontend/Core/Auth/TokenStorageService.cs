using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace LTSFrontend.Core.Auth
{
    public class TokenStorageService(ProtectedLocalStorage _storage, ILogger<TokenStorageService> _logger) : ITokenStorageService
    {
        private const string StorageKey = "lts_session";
        public async Task SaveSessionAsync(StoredSession session)
        {
            try
            {
                await _storage.SetAsync(StorageKey, session);
                _logger.LogInformation(
                    "[TokenStorage] Saved session for UserID={UserId}, AccessTokenExpiry={Expiry:o}, HasRefreshToken={HasRt}",
                    session.UserID, session.AccessTokenExpiry, !string.IsNullOrWhiteSpace(session.RefreshToken));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[TokenStorage] SaveSessionAsync FAILED for UserID={UserId} - session will NOT survive a refresh. " +
                    "This is the #1 thing to check if refresh always logs the user out.",
                    session.UserID);
            }
        }

        public async Task<StoredSession?> GetSessionAsync()
        {
            try
            {
                var result = await _storage.GetAsync<StoredSession>(StorageKey);
                if (!result.Success || result.Value == null)
                {
                    _logger.LogInformation("[TokenStorage] GetSessionAsync: no stored session found (nothing to restore).");
                    return null;
                }

                _logger.LogInformation(
                    "[TokenStorage] GetSessionAsync: restored UserID={UserId}, AccessTokenExpiry={Expiry:o} (now={Now:o}), HasRefreshToken={HasRt}",
                    result.Value.UserID, result.Value.AccessTokenExpiry, DateTime.UtcNow, !string.IsNullOrWhiteSpace(result.Value.RefreshToken));
                return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[TokenStorage] GetSessionAsync FAILED (exception) - treating as no stored session. " +
                    "If this fires on every single refresh, JS interop or Data Protection is the culprit, not timing.");
                try { await ClearSessionAsync(); } catch { /* best-effort */ }
                return null;
            }
        }

        public async Task ClearSessionAsync()
        {
            try
            {
                await _storage.DeleteAsync(StorageKey);
            }
            catch
            {
                // JS interop not available - ignore.
            }
        }
    }
}
