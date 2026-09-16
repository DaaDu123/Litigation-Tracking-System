using LTSBackend.Data;
using LTSBackend.Features.Roles.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace LTSBackend.Features.Roles.Queries.GetAllRoles;

public class GetAllRolesHandler(AppDbContext _context, ILogger<GetAllRolesHandler> _logger) : IRequestHandler<GetAllRolesQuery, List<RoleDTO>>
{
    // =====================================================
    // HANDLE — lists every role on the platform with its granted permissions
    // =====================================================
    public async Task<List<RoleDTO>> Handle(GetAllRolesQuery request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all roles");

        var roles = await _context.Roles
            .AsNoTracking()
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .Select(x => new RoleDTO
            {
                RoleID = x.RoleID,
                RoleName = x.RoleName,
                Description = x.Description,
                Permissions = x.RolePermissions
                    .Select(rp => new RolePermissionDTO
                    {
                        PermissionID = rp.PermissionID,
                        PermissionName = rp.Permission!.PermissionName
                    })
                    .ToList()
            })
            .OrderBy(x => x.RoleName)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} roles", roles.Count);

        return roles;
    }
}