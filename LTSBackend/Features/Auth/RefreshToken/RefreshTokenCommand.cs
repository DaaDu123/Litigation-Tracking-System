using MediatR;
namespace LTSBackend.Features.Auth.RefreshToken;

public record RefreshTokenCommand() : IRequest<RefreshTokenResponseDTO>;