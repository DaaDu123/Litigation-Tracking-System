namespace LTSBackend.Features.Auth.ResendOtp;

public class ResendOtpResponseDTO
{
    public string Email { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}