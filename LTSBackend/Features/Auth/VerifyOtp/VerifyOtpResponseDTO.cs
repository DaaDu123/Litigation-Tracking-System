namespace LTSBackend.Features.Auth.VerifyOtp
{
    public class VerifyOtpResponseDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? RoleName { get; set; }

        public string AccessToken { get; set; } = string.Empty;

        public DateTime AccessTokenExpiry { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}