using LTSBackend.Features.Permissions.DTOs;
using MediatR;

namespace LTSBackend.Features.Permissions.Queries.GetAllPermissions;

public record GetAllPermissionsQuery : IRequest<List<PermissionDTO>>;