namespace LTSBackend.Features.Auth.ResetPassword;

public class ResetPasswordResponseDTO
{
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}