namespace LTSFrontend.State
{





    public class UserSessionState
    {
        public int UserID { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string? Role { get; private set; }
        public string? AccessToken { get; private set; }
        public DateTime? AccessTokenExpiry { get; private set; }

        public bool IsAuthenticated =>
            UserID != 0 && !string.IsNullOrWhiteSpace(AccessToken);

        public event Action? OnChange;

        public void Set(int userId, string fullName, string email, string? role, string accessToken, DateTime accessTokenExpiry)
        {
            UserID = userId;
            FullName = fullName;
            Email = email;
            Role = role;
            AccessToken = accessToken;
            AccessTokenExpiry = accessTokenExpiry;
            NotifyStateChanged();
        }

        public void UpdateAccessToken(string accessToken, DateTime accessTokenExpiry)
        {
            AccessToken = accessToken;
            AccessTokenExpiry = accessTokenExpiry;
            NotifyStateChanged();
        }


        public void UpdateFullName(string fullName)
        {
            FullName = fullName;
            NotifyStateChanged();
        }

        public void Clear()
        {
            UserID = 0;
            FullName = string.Empty;
            Email = string.Empty;
            Role = null;
            AccessToken = null;
            AccessTokenExpiry = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
