namespace LTSFrontend.Features.Auth.DTOs
{
    public class ResendOtpResponseDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
