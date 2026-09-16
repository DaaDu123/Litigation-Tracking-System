using MediatR;

namespace LTSBackend.Features.Auth.Logout;

public record LogoutCommand : IRequest<bool>;