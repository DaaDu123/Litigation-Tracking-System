using MediatR;

namespace LTSBackend.Features.Users.Commands.ChangeUserRole;

public record ChangeUserRoleCommand(int UserID, int NewRoleID) : IRequest<bool>;
