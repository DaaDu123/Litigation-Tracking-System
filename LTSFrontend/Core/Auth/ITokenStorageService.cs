namespace LTSFrontend.Core.Auth
{





    public record StoredSession(int UserID,string FullName,string Email,string? Role,string AccessToken,DateTime AccessTokenExpiry,string? RefreshToken = null);
    public interface ITokenStorageService
    {
        Task SaveSessionAsync(StoredSession session);
        Task<StoredSession?> GetSessionAsync();
        Task ClearSessionAsync();
    }
}
