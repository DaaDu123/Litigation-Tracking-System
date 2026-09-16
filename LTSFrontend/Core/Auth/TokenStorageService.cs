using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Cryptography;

namespace LTSFrontend.Core.Auth
{
    /// <summary>
    /// Persists the logged-in session in the browser (encrypted) using
    /// Blazor Server's ProtectedLocalStorage, so a page refresh doesn't
    /// force the user to log in again.
    ///
    /// NOTE: ProtectedLocalStorage needs an active JS runtime, which is
    /// only available *after* the interactive circuit connects. Calling
    /// it during static/prerender will throw an InvalidOperationException
    /// - we swallow that here and simply behave as "nothing stored yet".
    /// Callers should read the session from OnAfterRenderAsync(firstRender).
    /// </summary>
    public class TokenStorageService(ProtectedLocalStorage _storage) : ITokenStorageService
    {
        private const string StorageKey = "lts_session";
        public async Task SaveSessionAsync(StoredSession session)
        {
            try
            {
                await _storage.SetAsync(StorageKey, session);
            }
            catch (InvalidOperationException)
            {
                // JS interop not available yet (prerendering) - ignore.
            }
        }

        public async Task<StoredSession?> GetSessionAsync()
        {
            try
            {
                var result = await _storage.GetAsync<StoredSession>(StorageKey);
                if (result.Success)
                {
                    return result.Value;
                }
                else
                {
                    return null;
                }
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (CryptographicException)
            {
                // Storage key rotated / corrupted value - clean it up.
                await ClearSessionAsync();
                return null;
            }
        }

        public async Task ClearSessionAsync()
        {
            try
            {
                await _storage.DeleteAsync(StorageKey);
            }
            catch (InvalidOperationException)
            {
                // JS interop not available yet - ignore.
            }
        }
    }
}
