namespace LTSFrontend.Features.LoginHistory.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.LoginHistory.DTOs.LoginHistoryDTO</summary>
    public class LoginHistoryDTO
    {
        public int LoginID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirmName { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsLoggedOut { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
