using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Middleware;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Permissions.Commands.AssignPermissions;

public class AssignPermissionsHandler : IRequestHandler<AssignPermissionsCommand, bool>
{
    private readonly AppDbContext _context;
    private readonly ILogger<AssignPermissionsHandler> _logger;

    private static readonly string[] ProtectedRoles =
    {
        RoleNames.SuperAdmin,
        RoleNames.FirmAdmin
    };

    public AssignPermissionsHandler(AppDbContext context, ILogger<AssignPermissionsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }












    public async Task<bool> Handle(AssignPermissionsCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning {Count} permissions to role: {RoleID}",request.PermissionIds.Count,request.RoleID);


        var permissionIds = request.PermissionIds
            .Distinct()
            .ToList();


        var role = await _context.Roles
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.RoleID == request.RoleID,cancellationToken);

        if (role == null)
        {
            _logger.LogWarning("Assign permissions failed: Role not found: {RoleID}", request.RoleID);
            throw new NotFoundException("Role not found.");
        }


        if (ProtectedRoles.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Assign permissions blocked: {RoleName} is a protected system role: {RoleID}", role.RoleName, request.RoleID);
            throw new ValidationException(
                new List<string>
                {
                    $"Permissions for system role '{role.RoleName}' cannot be modified through this endpoint."
                });
        }


        var validPermissionIds = await _context.Permissions
            .Where(x => permissionIds.Contains(x.PermissionID))
            .Select(x => x.PermissionID)
            .ToListAsync(cancellationToken);

        if (permissionIds.Count != validPermissionIds.Count)
        {
            _logger.LogWarning("Assign permissions failed: Invalid permissions for role: {RoleID}",request.RoleID);

            throw new ValidationException(new List<string> { "One or more permissions are invalid." });
        }




        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {

                _context.RolePermissions.RemoveRange(role.RolePermissions);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Removed {Count} old permissions for role: {RoleID}",
                    role.RolePermissions.Count, request.RoleID);


                var rolePermissions = permissionIds
                    .Select(permissionId => new RolePermission
                    {
                        RoleID = request.RoleID,
                        PermissionID = permissionId
                    });

                await _context.RolePermissions.AddRangeAsync(rolePermissions,cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Assigned {Count} new permissions to role: {RoleID}",permissionIds.Count, request.RoleID);


                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Permissions assigned successfully to role: {RoleID}", request.RoleID);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError("Assign permissions failed - transaction rolled back");
                throw;
            }
        });
    }
}