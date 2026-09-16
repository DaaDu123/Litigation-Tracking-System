using LTSBackend.Comman.Enum;
using LTSBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.Permissions;

public class PermissionService(AppDbContext _context, ILogger<PermissionService> _logger) : IPermissionService
{
    // Fixed, hard-coded permission set for SuperAdmin (the platform owner).
    // Deliberately NOT "every permission" - see the Roles SRS: SuperAdmin's
    // job is workspace provisioning, FirmAdmin account custody, data
    // export/migration, and immutable system-wide audit logging. Anything
    // that belongs to a firm's internal operation (cases, documents,
    // hearings, master data, non-FirmAdmin user management) is intentionally
    // excluded, even though these permission strings otherwise resemble
    // "PermissionEnum" / [HasPermission("...")] names used elsewhere.
    // ManageRoles/ManageSystemUsers are included because Role/RolePermission
    // and the FirmAdmin account lifecycle are global, non-tenant-scoped
    // concerns with no other legitimate owner - not because SuperAdmin is
    // meant to have broad reach.
    private static readonly HashSet<string> SuperAdminPermissions = new(StringComparer.Ordinal)
    {
        nameof(PermissionEnum.ManageFirms),          // Firm workspace provisioning/blocking/removal
        nameof(PermissionEnum.ManageSystemUsers),     // Create/update/remove FirmAdmin accounts only
        nameof(PermissionEnum.ManageDataMigration),   // Data export / domain migration
        nameof(PermissionEnum.ViewSystemAuditLogs),   // System-wide audit visibility (all firms)
        nameof(PermissionEnum.ViewAuditLogs),         // AuditLogsController policy name - unfiltered for SuperAdmin
        nameof(PermissionEnum.ViewLoginHistory),      // Login-attempt audit trail, all firms
        nameof(PermissionEnum.ViewDashboard),         // SuperAdmin's own platform dashboard
        "ManageRoles",                                // Global RBAC config - see RolesController for why
    };


    // =====================================================
    // HAS PERMISSION ASYNC
    // Checks whether a user holds a specific permission. Denies (returns
    // false) for missing/roleless/inactive/deleted/blocked-firm users, so
    // a permission check can never silently "pass" for an account that
    // should not be able to act at all - this is enforced here in
    // addition to the JWT-level active-status check (Program.cs
    // OnTokenValidated) as defence in depth.
    // =====================================================
    public async Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Load the user with role + role-permissions + firm status.
            // IgnoreQueryFilters(): this service is also used by the
            // authorization handler itself, so it must be able to see
            // the record regardless of the caller's own tenant scope in
            // order to correctly evaluate it (and then explicitly reject
            // inactive/blocked accounts below, rather than have them
            // silently disappear behind a filter).
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

            // 2. Reject deactivated, soft-deleted, or blocked-firm accounts.
            // A valid, unexpired JWT does not by itself mean the account
            // is still allowed to act - Firm Admins can deactivate/block
            // users at any time, and Super Admin can suspend a firm.
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

            // 3. Super Admin holds ONLY the fixed platform-owner permission
            // set below - NOT every permission. Super Admin is scoped to:
            // workspace/firm provisioning, FirmAdmin account management,
            // system-wide audit/login-history visibility, the platform
            // dashboard, and (because Role/RolePermission are global,
            // non-tenant-scoped tables with no other legitimate owner -
            // see RolesController/PermissionsController) RBAC config.
            // Everything firm-internal (cases, documents, hearings,
            // master data, firm-level user management) is explicitly
            // OUT of scope and falls through to "denied" below, same as
            // any other role that lacks the permission.
            if (user.GetRole() == UserRole.SuperAdmin)
            {
                bool superAdminAllowed = SuperAdminPermissions.Contains(permission);
                _logger.LogDebug("Super Admin permission check for {Permission}: {Result}", permission, superAdminAllowed);
                return superAdminAllowed;
            }

            // 4. Otherwise check the role's granted permissions.
            // Guard against rp.Permission being null (e.g. an orphaned
            // RolePermission row) instead of letting a NullReferenceException
            // bubble up - this also clears the CS8602 nullable warning, since
            // RolePermission.Permission is a nullable navigation property.
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

    // =====================================================
    // GET PERMISSIONS ASYNC
    // Returns the full list of permission names available to the user
    // (the fixed platform-owner set for SuperAdmin, otherwise the role's
    // granted permission set).
    // =====================================================
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

            // Super Admin - the fixed platform-owner set only (not every
            // permission - see SuperAdminPermissions above).
            if (user.GetRole() == UserRole.SuperAdmin)
            {
                return SuperAdminPermissions.OrderBy(x => x).ToList();
            }

            // Otherwise, the role's granted permissions.
            // Same null-guard as HasPermissionAsync above - skip any
            // RolePermission row whose Permission navigation is null rather
            // than risk a NullReferenceException (and clear the CS8602
            // nullable warning).
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

    // =====================================================
    // HAS ROLE ASYNC
    // Checks whether the user's currently assigned role matches the given
    // role.
    // =====================================================
    public async Task<bool> HasRoleAsync(int userId, UserRole role, CancellationToken cancellationToken = default)
    {
        try
        {
            // Fetch RoleID (a plain int, translatable to SQL) first, then convert
            // to the UserRole enum in memory - GetRole() itself cannot be
            // translated by EF Core's SQL provider.
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

    // =====================================================
    // GET USER ROLE ASYNC
    // Returns the user's current role as an enum, or null if
    // unset/unrecognised.
    // =====================================================
    public async Task<UserRole?> GetUserRoleAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Same pattern as HasRoleAsync: fetch the primitive RoleID via SQL,
            // then convert to the enum in memory.
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

    // =====================================================
    // HAS FULL CASE DIRECTORY VISIBILITY ASYNC
    // SRS "View Firm Case Directory": a plain permission check against
    // ViewFirmCaseDirectory (granted to FirmAdmin and Partner in
    // SeedRolePermissions). SuperAdmin is deliberately NOT special-cased
    // here - case data is firm-internal business, out of scope for the
    // platform owner - so this falls through to the same permission check
    // as everyone else, which correctly denies SuperAdmin.
    // =====================================================
    public async Task<bool> HasFullCaseDirectoryVisibilityAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await HasPermissionAsync(userId, nameof(PermissionEnum.ViewFirmCaseDirectory), cancellationToken);
    }

    // =====================================================
    // IS USER ASSIGNED TO CASE ASYNC
    // SRS RBAC "Case assignment" check: confirms both that the case
    // belongs to the user's own firm (tenant isolation) AND that an
    // active CaseAssignment row links this user to it. Deliberately does
    // NOT treat SuperAdmin/full-directory-visibility roles as "assigned"
    // - callers that want those roles to bypass the assignment check
    // should call HasFullCaseDirectoryVisibilityAsync first.
    // =====================================================
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

            // Tenant isolation: the case must belong to the same firm as the
            // user before assignment is even considered.
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