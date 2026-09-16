using MediatR;
namespace LTSBackend.Features.Roles.Commands.CreateRole;

public sealed record CreateRoleCommand(string RoleName, string? Description, List<int> PermissionIds) : IRequest<int>;