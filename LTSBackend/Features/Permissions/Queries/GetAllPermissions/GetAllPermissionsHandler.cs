using LTSBackend.Data;
using LTSBackend.Features.Permissions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Permissions.Queries.GetAllPermissions;

public class GetAllPermissionsHandler(AppDbContext context) : IRequestHandler<GetAllPermissionsQuery, List<PermissionDTO>>
{
    // =====================================================
    // HANDLE — lists every permission defined on the platform
    // =====================================================
    public async Task<List<PermissionDTO>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await context.Permissions
            .AsNoTracking()
            .OrderBy(x => x.PermissionName)
            .Select(x => new PermissionDTO
            {
                PermissionID = x.PermissionID,
                PermissionName = x.PermissionName
            }).ToListAsync(cancellationToken);
    }
}