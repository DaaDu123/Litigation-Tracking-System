using LTSBackend.Features.Permissions.DTOs;
using MediatR;

namespace LTSBackend.Features.Permissions.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(int RoleID) : IRequest<List<PermissionDTO>>;