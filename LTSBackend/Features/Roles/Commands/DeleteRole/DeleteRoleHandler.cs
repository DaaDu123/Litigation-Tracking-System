using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Middleware;
using LTSBackend.Data;
using LTSBackend.Features.Roles.Commands.DeleteRole;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly AppDbContext _context;
    private readonly ILogger<DeleteRoleHandler> _logger;

    private static readonly string[] ProtectedRoles =
    {
        RoleNames.SuperAdmin,
        RoleNames.FirmAdmin
    };

    public DeleteRoleHandler(AppDbContext context, ILogger<DeleteRoleHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =====================================================
    // HANDLE — removes a platform-wide role definition
    // Refuses to delete a protected system role (SuperAdmin/FirmAdmin) or
    // a role that still has users assigned to it, then inside one
    // retry-safe transaction removes its RolePermissions rows before
    // removing the role itself.
    // =====================================================
    public async Task<bool> Handle(DeleteRoleCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting role: {RoleID}", request.RoleID);

        // 1. Find role with users and permissions
        var role = await _context.Roles
            .Include(x => x.Users)
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.RoleID == request.RoleID,cancellationToken);

        if (role == null)
        {
            _logger.LogWarning("Delete failed: Role not found: {RoleID}", request.RoleID);
            throw new NotFoundException("Role not found.");
        }

        // 2. Protect system roles
        if (ProtectedRoles.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Delete failed: System role cannot be deleted: {RoleID}", request.RoleID);
            throw new ValidationException(new List<string> { $"System role '{role.RoleName}' cannot be deleted." });
        }

        // 3. Check if users are assigned to this role
        if (role.Users.Any())
        {
            _logger.LogWarning("Delete failed: {Count} users assigned to role: {RoleID}",role.Users.Count,request.RoleID);

            throw new ValidationException(new List<string> {$"Cannot delete role. {role.Users.Count} user(s) are assigned to this role."});
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
                // 5. Remove all permissions for this role
                _context.RolePermissions.RemoveRange(role.RolePermissions);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Removed {Count} permissions for role: {RoleID}",role.RolePermissions.Count,request.RoleID);

                // 6. Delete role
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Role permissions deleted, removing role: {RoleID}", request.RoleID);

                // 7. Commit transaction
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Role deleted successfully: {RoleID}", request.RoleID);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError("Role deletion failed - transaction rolled back");
                throw;
            }
        });
    }
}