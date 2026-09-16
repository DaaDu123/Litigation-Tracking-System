namespace LTSFrontend.Core.Auth
{
    /// <summary>
    /// A small record of everything we need to restore a session
    /// after the browser is refreshed (F5) without asking the user
    /// to log in again, as long as the access token is still valid.
    /// </summary>
    public record StoredSession(int UserID,string FullName,string Email,string? Role,string AccessToken,DateTime AccessTokenExpiry);
    public interface ITokenStorageService
    {
        Task SaveSessionAsync(StoredSession session);
        Task<StoredSession?> GetSessionAsync();
        Task ClearSessionAsync();
    }
}
