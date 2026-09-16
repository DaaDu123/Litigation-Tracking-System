using MediatR;
namespace LTSBackend.Features.Roles.Commands.UpdateRole;

public sealed record UpdateRoleCommand(int RoleID, string RoleName, string? Description, List<int> PermissionIds) : IRequest<bool>;