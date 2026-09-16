using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Middleware;
using LTSBackend.Data;
using LTSBackend.Features.Roles.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace LTSBackend.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdHandler (AppDbContext _context, ILogger<GetRoleByIdHandler> _logger) : IRequestHandler<GetRoleByIdQuery, RoleDTO>
{
    // =====================================================
    // HANDLE — fetches a single role's details and granted permissions
    // =====================================================
    public async Task<RoleDTO> Handle(GetRoleByIdQuery request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching role: {RoleID}", request.RoleID);

        var role = await _context.Roles
            .AsNoTracking()
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.RoleID == request.RoleID,cancellationToken);

        if (role == null)
        {
            _logger.LogWarning("Role not found: {RoleID}", request.RoleID);
            throw new NotFoundException("Role not found.");
        }

        var roleDto = new RoleDTO
        {
            RoleID = role.RoleID,
            RoleName = role.RoleName,
            Description = role.Description,
            Permissions = role.RolePermissions
                .Select(rp => new RolePermissionDTO
                {
                    PermissionID = rp.PermissionID,
                    PermissionName = rp.Permission!.PermissionName
                })
                .ToList()
        };

        _logger.LogInformation("Role retrieved: {RoleID}", request.RoleID);

        return roleDto;
    }
}