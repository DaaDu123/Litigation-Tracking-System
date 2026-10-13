using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Middleware;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Roles.Commands.UpdateRole;

public class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, bool>
{
    private readonly AppDbContext _context;
    private readonly ILogger<UpdateRoleHandler> _logger;

    private static readonly string[] ProtectedRoles =
       {
        RoleNames.SuperAdmin,
        RoleNames.FirmAdmin
    };
    public UpdateRoleHandler(AppDbContext context, ILogger<UpdateRoleHandler> logger)
    {
        _context = context;
        _logger = logger;
    }








    public async Task<bool> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating role: {RoleID}", request.RoleID);


        request = request with
        {
            RoleName = request.RoleName.Trim(),
            PermissionIds = request.PermissionIds
                .Distinct()
                .ToList()
        };


        var role = await _context.Roles
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.RoleID == request.RoleID,cancellationToken);

        if (role == null)
        {
            _logger.LogWarning("Update failed: Role not found: {RoleID}", request.RoleID);
            throw new NotFoundException("Role not found.");
        }


        bool roleExists = await _context.Roles
            .AnyAsync(x =>x.RoleID != request.RoleID && x.RoleName.ToLower() == request.RoleName.ToLower(),cancellationToken);

        if (roleExists)
        {
            _logger.LogWarning("Update failed: Role name already exists: {RoleName}", request.RoleName);
            throw new ValidationException(new List<string> { $"Role '{request.RoleName}' already exists." });
        }

        if (ProtectedRoles.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Update blocked: {RoleName} is a protected system role: {RoleID}", role.RoleName, request.RoleID);
            throw new ValidationException(
                new List<string>
                {
                    $"System role '{role.RoleName}' cannot be updated through this endpoint."
                });
        }


        var validPermissions = await _context.Permissions
            .Where(x => request.PermissionIds.Contains(x.PermissionID))
            .Select(x => x.PermissionID)
            .ToListAsync(cancellationToken);

        if (validPermissions.Count != request.PermissionIds.Count)
        {
            _logger.LogWarning("Update failed: Invalid permissions for role: {RoleID}", request.RoleID);
            throw new ValidationException(new List<string> { "One or more permissions are invalid." });
        }





        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {

                role.RoleName = request.RoleName;
                role.Description = request.Description;


                _context.RolePermissions.RemoveRange(role.RolePermissions);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Removed old permissions for role: {RoleID}", request.RoleID);


                var rolePermissions = validPermissions.Select(permissionId => new RolePermission
                    {
                        RoleID = role.RoleID,
                        PermissionID = permissionId
                    });

                await _context.RolePermissions.AddRangeAsync(rolePermissions,cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Role {RoleID} assigned {Count} new permissions",request.RoleID,validPermissions.Count);


                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Role updated successfully: {RoleID}", request.RoleID);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError("Role update failed - transaction rolled back");
                throw;
            }
        });
    }
}
