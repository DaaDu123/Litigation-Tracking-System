using MediatR;

namespace LTSBackend.Features.Users.Commands.UnblockFirmUser;

public record UnblockFirmUserCommand(int UserID) : IRequest<bool>;
