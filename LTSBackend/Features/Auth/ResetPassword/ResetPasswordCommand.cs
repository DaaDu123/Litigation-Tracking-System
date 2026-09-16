using MediatR;
namespace LTSBackend.Features.Auth.ResetPassword;

public record ResetPasswordCommand(string Email, string OtpCode, string NewPassword) : IRequest<ResetPasswordResponseDTO>;
