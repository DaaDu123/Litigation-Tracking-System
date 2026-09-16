using MediatR;

namespace LTSBackend.Features.Permissions.Commands.AssignPermissions;

public sealed record AssignPermissionsCommand(
    int RoleID,
    List<int> PermissionIds
) : IRequest<bool>;