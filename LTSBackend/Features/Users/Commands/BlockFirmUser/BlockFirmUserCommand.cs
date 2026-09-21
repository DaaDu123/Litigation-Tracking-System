using MediatR;

namespace LTSBackend.Features.Users.Commands.BlockFirmUser;

public record BlockFirmUserCommand(int UserID, string Reason) : IRequest<bool>;
