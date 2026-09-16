using MediatR;

namespace LTSBackend.Features.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResponseDTO>;