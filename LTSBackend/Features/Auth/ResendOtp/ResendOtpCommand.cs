using MediatR;
namespace LTSBackend.Features.Auth.ResendOtp;

public record ResendOtpCommand(string Email) : IRequest<ResendOtpResponseDTO>;
