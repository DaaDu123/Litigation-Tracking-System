using MediatR;
namespace LTSBackend.Features.Auth.VerifyOtp
{
    public record VerifyOtpCommand(string Email, string OtpCode) : IRequest<VerifyOtpResponseDTO>;
}