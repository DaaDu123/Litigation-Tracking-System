using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Permissions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Permissions.Queries.GetRolePermissions;

public class GetRolePermissionsHandler(AppDbContext context) : IRequestHandler<GetRolePermissionsQuery, List<PermissionDTO>>
{
    // =====================================================
    // HANDLE — lists the permissions currently granted to a specific role
    // =====================================================
    public async Task<List<PermissionDTO>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        bool roleExists = await context.Roles.AnyAsync(x => x.RoleID == request.RoleID, cancellationToken);

        if (!roleExists)
            throw new NotFoundException("Role not found.");

        return await context.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleID == request.RoleID)
            .Include(x => x.Permission)
            .Select(x => new PermissionDTO
            {
                PermissionID = x.PermissionID,
                PermissionName = x.Permission!.PermissionName
            }).ToListAsync(cancellationToken);
    }
}