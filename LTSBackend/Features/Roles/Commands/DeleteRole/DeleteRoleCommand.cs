using MediatR;
namespace LTSBackend.Features.Roles.Commands.DeleteRole;

public sealed record DeleteRoleCommand(int RoleID) : IRequest<bool>;