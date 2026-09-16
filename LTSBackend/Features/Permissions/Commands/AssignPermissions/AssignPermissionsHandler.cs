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

    // =====================================================
    // HANDLE — replaces a role's entire permission set platform-wide
    // Refuses to touch a protected system role (SuperAdmin/FirmAdmin —
    // their permissions aren't editable through this endpoint), validates
    // every supplied PermissionID actually exists, then inside one
    // retry-safe transaction removes all of the role's current
    // RolePermissions rows and inserts the new set. See the class-level
    // note on RolesController/PermissionsController for why this whole
    // operation is intentionally SuperAdmin-only (Role/RolePermission are
    // global, non-tenant-scoped tables).
    // =====================================================
    public async Task<bool> Handle(AssignPermissionsCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning {Count} permissions to role: {RoleID}",request.PermissionIds.Count,request.RoleID);

        // 1. Normalize permission IDs (deduplicate)
        var permissionIds = request.PermissionIds
            .Distinct()
            .ToList();

        // 2. Find role with current permissions
        var role = await _context.Roles
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.RoleID == request.RoleID,cancellationToken);

        if (role == null)
        {
            _logger.LogWarning("Assign permissions failed: Role not found: {RoleID}", request.RoleID);
            throw new NotFoundException("Role not found.");
        }

        // FIX: Protected system roles ki permissions modify na hone dein
        if (ProtectedRoles.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Assign permissions blocked: {RoleName} is a protected system role: {RoleID}", role.RoleName, request.RoleID);
            throw new ValidationException(
                new List<string>
                {
                    $"Permissions for system role '{role.RoleName}' cannot be modified through this endpoint."
                });
        }

        // 3. Validate all permissions exist
        var validPermissionIds = await _context.Permissions
            .Where(x => permissionIds.Contains(x.PermissionID))
            .Select(x => x.PermissionID)
            .ToListAsync(cancellationToken);

        if (permissionIds.Count != validPermissionIds.Count)
        {
            _logger.LogWarning("Assign permissions failed: Invalid permissions for role: {RoleID}",request.RoleID);

            throw new ValidationException(new List<string> { "One or more permissions are invalid." });
        }

        // 4. Begin transaction (wrapped in CreateExecutionStrategy since
        // EnableRetryOnFailure is on - see CreateFirmCommandHandler for
        // the full explanation).
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 5. Remove old permissions
                _context.RolePermissions.RemoveRange(role.RolePermissions);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Removed {Count} old permissions for role: {RoleID}",
                    role.RolePermissions.Count, request.RoleID);

                // 6. Assign new permissions
                var rolePermissions = permissionIds
                    .Select(permissionId => new RolePermission
                    {
                        RoleID = request.RoleID,
                        PermissionID = permissionId
                    });

                await _context.RolePermissions.AddRangeAsync(rolePermissions,cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Assigned {Count} new permissions to role: {RoleID}",permissionIds.Count, request.RoleID);

                // 7. Commit transaction
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