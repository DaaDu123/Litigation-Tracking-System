using LTSBackend.Comman.Enum;
using LTSBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.Permissions;

public class PermissionService(AppDbContext _context, ILogger<PermissionService> _logger) : IPermissionService
{












    private static readonly HashSet<string> SuperAdminPermissions = new(StringComparer.Ordinal)
    {
        nameof(PermissionEnum.ManageFirms),
        nameof(PermissionEnum.ManageSystemUsers),
        nameof(PermissionEnum.ManageDataMigration),
        nameof(PermissionEnum.ViewSystemAuditLogs),
        nameof(PermissionEnum.ViewAuditLogs),
        nameof(PermissionEnum.ViewLoginHistory),
        nameof(PermissionEnum.ViewDashboard),
        "ManageRoles",
    };











    public async Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default)
    {
        try
        {







            var user = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(x => x.Role)
                .ThenInclude(x => x!.RolePermissions)
                .ThenInclude(x => x.Permission)
                .Include(x => x.Firm)
                .FirstOrDefaultAsync(x => x.UserID == userId, cancellationToken);

            if (user == null || user.Role == null)
            {
                _logger.LogWarning("Permission check denied - user not found or has no role: {UserId}", userId);
                return false;
            }





            if (user.IsDeleted || !user.IsActive)
            {
                _logger.LogWarning("Permission check denied - account inactive or deleted: {UserId}", userId);
                return false;
            }

            if (user.Firm != null && (user.Firm.IsBlocked || user.Firm.IsDeleted))
            {
                _logger.LogWarning("Permission check denied - firm is blocked/deleted for user {UserId}", userId);
                return false;
            }












            if (user.GetRole() == UserRole.SuperAdmin)
            {
                bool superAdminAllowed = SuperAdminPermissions.Contains(permission);
                _logger.LogDebug("Super Admin permission check for {Permission}: {Result}", permission, superAdminAllowed);
                return superAdminAllowed;
            }






            bool hasPermission = user.Role.RolePermissions.Any(rp => rp.Permission != null && rp.Permission.PermissionName == permission);

            _logger.LogDebug("Permission check for user {UserId} on {Permission}: {Result}", userId, permission, hasPermission);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking permission {Permission} for user {UserId}", permission, userId);
            return false;
        }
    }







    public async Task<List<string>> GetPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(x => x.Role)
                .ThenInclude(x => x!.RolePermissions)
                .ThenInclude(x => x.Permission)
                .FirstOrDefaultAsync(x => x.UserID == userId, cancellationToken);

            if (user == null || user.Role == null || user.IsDeleted || !user.IsActive)
            {
                return [];
            }



            if (user.GetRole() == UserRole.SuperAdmin)
            {
                return SuperAdminPermissions.OrderBy(x => x).ToList();
            }






            var permissions = user.Role.RolePermissions
                .Where(x => x.Permission != null)
                .Select(x => x.Permission!.PermissionName)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            _logger.LogInformation("User {UserId} has {Count} permissions", userId, permissions.Count);

            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching permissions for user {UserId}", userId);
            return [];
        }
    }






    public async Task<bool> HasRoleAsync(int userId, UserRole role, CancellationToken cancellationToken = default)
    {
        try
        {



            var roleId = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.UserID == userId)
                .Select(x => x.RoleID)
                .FirstOrDefaultAsync(cancellationToken);

            if (!roleId.HasValue || !Enum.IsDefined(typeof(UserRole), roleId.Value))
            {
                return false;
            }

            var userRole = (UserRole)roleId.Value;
            return userRole == role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking role for user {UserId}", userId);
            return false;
        }
    }






    public async Task<UserRole?> GetUserRoleAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {


            var roleId = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.UserID == userId)
                .Select(x => x.RoleID)
                .FirstOrDefaultAsync(cancellationToken);

            if (!roleId.HasValue || !Enum.IsDefined(typeof(UserRole), roleId.Value))
            {
                return null;
            }

            return (UserRole)roleId.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching role for user {UserId}", userId);
            return null;
        }
    }










    public async Task<bool> HasFullCaseDirectoryVisibilityAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await HasPermissionAsync(userId, nameof(PermissionEnum.ViewFirmCaseDirectory), cancellationToken);
    }










    public async Task<bool> IsUserAssignedToCaseAsync(int userId, long caseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserID == userId, cancellationToken);

            if (user == null || user.IsDeleted || !user.IsActive)
                return false;



            var caseFirmId = await _context.Cases
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.CaseID == caseId)
                .Select(c => (int?)c.FirmID)
                .FirstOrDefaultAsync(cancellationToken);

            if (caseFirmId == null || caseFirmId != user.FirmID)
            {
                _logger.LogWarning("User {UserId} (Firm {UserFirmId}) denied cross-firm case-assignment check on case {CaseId} (Firm {CaseFirmId})",
                    userId, user.FirmID, caseId, caseFirmId);
                return false;
            }

            var now = DateTime.UtcNow;
            bool isAssigned = await _context.CaseAssignments
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(a => a.CaseID == caseId && a.UserID == userId && (a.EndDate == null || a.EndDate > now), cancellationToken);

            return isAssigned;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking case assignment for user {UserId} case {CaseId}", userId, caseId);
            return false;
        }
    }
}