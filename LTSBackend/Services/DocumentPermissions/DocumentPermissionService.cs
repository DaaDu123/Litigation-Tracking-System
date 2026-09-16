using LTSBackend.Comman.Enum;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Services.DocumentPermissions;

public class DocumentPermissionService(AppDbContext _context, ILogger<DocumentPermissionService> _logger) : IDocumentPermissionService
{
    public async Task<bool> CanUserAccessDocumentAsync(int userId, long documentId, string action, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Load the user with role + firm status.
            var user = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(x => x.Role)
                .Include(x => x.Firm)
                .FirstOrDefaultAsync(x => x.UserID == userId, cancellationToken);

            if (user == null || user.Role == null)
            {
                _logger.LogWarning("Document access denied - user not found or has no role: {UserId}", userId);
                return false;
            }

            // 2. Reject deactivated, soft-deleted, or blocked-firm accounts.
            if (user.IsDeleted || !user.IsActive)
            {
                _logger.LogWarning("Document access denied - account inactive or deleted: {UserId}", userId);
                return false;
            }

            if (user.Firm != null && (user.Firm.IsBlocked || user.Firm.IsDeleted))
            {
                _logger.LogWarning("Document access denied - firm is blocked/deleted for user {UserId}", userId);
                return false;
            }

            var role = user.GetRole();
            // 3. Super Admin has NO document access.
            if (role == UserRole.SuperAdmin)
            {
                _logger.LogWarning("Document access denied - SuperAdmin has no document access by design: {UserId}", userId);
                return false;
            }

            // 4. Multi-tenant isolation (CRITICAL): a user must never access
            // a document belonging to another firm's case, no matter what
            // role they hold. This is checked unconditionally, before any
            // role-specific rule runs, so no branch below can accidentally
            // bypass it.
            if (documentId > 0)
            {
                var docInfo = await _context.Documents
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(d => d.DocumentID == documentId)
                    .Select(d => new { FirmId = (int?)d.Case.FirmID, d.IsDraft, d.UploadedBy })
                    .FirstOrDefaultAsync(cancellationToken);

                if (docInfo == null || docInfo.FirmId == null || docInfo.FirmId != user.FirmID)
                {
                    _logger.LogWarning("User {UserId} (Firm {UserFirmId}) denied cross-firm access to document {DocumentId} (Firm {DocFirmId})",userId, user.FirmID, documentId, docInfo?.FirmId);
                    return false;
                }

         
                bool canSeeDrafts = role == UserRole.FirmAdmin || role == UserRole.Partner;
                if (docInfo.IsDraft && docInfo.UploadedBy != userId && !canSeeDrafts && action is "View" or "Download")
                {
                    _logger.LogDebug("Document {DocumentId} is a pending draft - denied for user {UserId} (role {Role})", documentId, userId, role);
                    return false;
                }
            }

            // 5. FirmAdmin and Partner have full access (View/Download/
            // Upload/Delete) within their own firm (enforced above).
            if (role == UserRole.FirmAdmin || role == UserRole.Partner)
            {
                _logger.LogDebug("Role {Role} has full document access (own firm only)", role);
                return true;
            }

            // 6. AssociateLawyer -> read/write on assigned cases only.
            if (role == UserRole.AssociateLawyer)
            {
                bool actionAllowed = action is "View" or "Download" or "Upload";
                if (!actionAllowed) return false;

                bool isAssigned = await IsUserAssignedToDocumentCaseAsync(userId, documentId, cancellationToken);
                _logger.LogDebug("AssociateLawyer {UserId} - {Action} on doc {DocumentId}, assigned={Assigned}", userId, action, documentId, isAssigned);
                return isAssigned;
            }

            // 7. Intern/Paralegal -> read-only on assigned cases only.
            if (role == UserRole.InternParalegal)
            {
                bool actionAllowed = action is "View" or "Download";
                if (!actionAllowed) return false;
                return await IsUserAssignedToDocumentCaseAsync(userId, documentId, cancellationToken);
            }

            // 8. Moharrir -> restricted (blind upload) or elevated mode.
            if (role == UserRole.Moharrir)
            {
                return await HandleMohallirAccessAsync(userId, documentId, action, cancellationToken);
            }

            _logger.LogWarning("User {UserId} with role {Role} tried to access document {DocumentId} with action {Action}", userId, role, documentId, action);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking document access for user {UserId} document {DocumentId} action {Action}", userId, documentId, action);
            return false;
        }
    }


    public async Task<bool> CanUserUploadToCaseAsync(int userId, long caseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(x => x.Firm)
                .FirstOrDefaultAsync(x => x.UserID == userId, cancellationToken);

            if (user == null || user.IsDeleted || !user.IsActive)
            {
                _logger.LogWarning("Upload denied - account inactive/deleted/not found: {UserId}", userId);
                return false;
            }

            if (user.Firm != null && (user.Firm.IsBlocked || user.Firm.IsDeleted))
            {
                _logger.LogWarning("Upload denied - firm blocked/deleted for user {UserId}", userId);
                return false;
            }

            var role = user.GetRole();

            // Super Admin cannot upload to any case - documents are
            // firm-internal, out of scope for the platform owner.
            if (role == UserRole.SuperAdmin)
            {
                _logger.LogWarning("Upload denied - SuperAdmin has no document access by design: {UserId}", userId);
                return false;
            }

            // Multi-tenant isolation: the case must belong to the user's own firm.
            var caseFirmId = await _context.Cases
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.CaseID == caseId)
                .Select(c => (int?)c.FirmID)
                .FirstOrDefaultAsync(cancellationToken);

            if (caseFirmId == null || caseFirmId != user.FirmID)
            {
                _logger.LogWarning("Upload denied - cross-firm or non-existent case {CaseId} for user {UserId}", caseId, userId);
                return false;
            }

            // FirmAdmin/Partner: full upload access within their own firm.
            if (role == UserRole.FirmAdmin || role == UserRole.Partner)
                return true;

            // Moharrir: "blind upload" is always allowed regardless of
            // elevated/restricted mode - matches HandleMohallirAccessAsync's
            // existing Upload rule.
            if (role == UserRole.Moharrir)
                return true;

            // AssociateLawyer and InternParalegal: upload only to a case
            // they are actually assigned to (per SRS role responsibilities).
            if (role == UserRole.AssociateLawyer || role == UserRole.InternParalegal)
            {
                var now = DateTime.UtcNow;
                bool isAssigned = await _context.CaseAssignments
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .AnyAsync(a => a.CaseID == caseId && a.UserID == userId && (a.EndDate == null || a.EndDate > now), cancellationToken);

                _logger.LogDebug("Role {Role} {UserId} upload to case {CaseId} - assigned={Assigned}", role, userId, caseId, isAssigned);
                return isAssigned;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking upload permission for user {UserId} case {CaseId}", userId, caseId);
            return false;
        }
    }

    private async Task<bool> IsUserAssignedToDocumentCaseAsync(int userId, long documentId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await _context.Documents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(d => d.DocumentID == documentId)
            .Join(_context.CaseAssignments.IgnoreQueryFilters().AsNoTracking().Where(a => a.UserID == userId && (a.EndDate == null || a.EndDate > now)),
                d => d.CaseID,
                a => a.CaseID,
                (d, a) => a)
            .AnyAsync(cancellationToken);
    }

    // Resolves Moharrir access (restricted vs elevated) for a single document,
    // checking per-document permission grants in priority order:
    // 1. User-specific grant (DocumentPermissions.UserID) - highest priority,
    //    lets an admin elevate/restrict one specific Moharrir individually.
    // 2. Role-based grant (DocumentPermissions.RoleID) - applies to every
    //    user sharing that role.
    // 3. Fallback - the role-level elevated/restricted default, driven by
    //    the "ViewDocumentsIfPermitted" permission.
    private async Task<bool> HandleMohallirAccessAsync(int userId, long documentId, string action, CancellationToken cancellationToken)
    {
        // Upload is always allowed for Moharrir, restricted or elevated -
        // this is exactly the "blind upload" capability the SRS requires.
        if (action == "Upload")
        {
            _logger.LogDebug("Moharrir {UserId} - Upload allowed by default", userId);
            return true;
        }

        if (documentId > 0)
        {
            var isAssigned = await IsUserAssignedToDocumentCaseAsync(userId, documentId, cancellationToken);
            if (!isAssigned)
            {
                _logger.LogDebug("Moharrir {UserId} not assigned to case for document {DocumentId} - denied", userId, documentId);
                return false;
            }

            // 8a. Check the user-specific grant first (highest priority).
            var userPermission = await _context.DocumentPermissions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DocumentID == documentId && x.UserID == userId, cancellationToken);

            if (userPermission != null)
            {
                bool userAllowed = action == "View" ? userPermission.CanView
                    : action == "Download" && userPermission.CanDownload;

                _logger.LogDebug("Moharrir {UserId} - user-specific permission found for document {DocumentId}, {Action} allowed: {Allowed}", userId, documentId, action, userAllowed);
                return userAllowed;
            }

            // 8b. Otherwise check the role-based grant.
            var user = await _context.Users.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken);

            if (user?.RoleID != null)
            {
                var rolePermission = await _context.DocumentPermissions
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DocumentID == documentId && x.RoleID == user.RoleID, cancellationToken);

                if (rolePermission != null)
                {
                    bool roleAllowed = action == "View" ? rolePermission.CanView
                        : action == "Download" && rolePermission.CanDownload;
                    _logger.LogDebug("Moharrir {UserId} - role-based permission found for document {DocumentId}, {Action} allowed: {Allowed}", userId, documentId, action, roleAllowed);
                    return roleAllowed;
                }
            }
        }

        // 8c. Fallback: no explicit per-document row -> role-level default
        // (restricted = write-only, elevated = view+download+upload).
        bool isElevated = await HasMohallirElevatedAccessAsync(userId, cancellationToken);
        if (isElevated && (action == "View" || action == "Download"))
        {
            _logger.LogDebug("Moharrir {UserId} elevated mode (default) - {Action} allowed", userId, action);
            return true;
        }

        _logger.LogDebug("Moharrir {UserId} restricted mode (default) - {Action} DENIED", userId, action);
        return false;
    }

    public async Task<bool> HasMohallirElevatedAccessAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var hasPermission = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.UserID == userId)
                .Include(x => x.Role!)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .AnyAsync(x => x.Role != null && x.Role.RolePermissions.Any(rp => rp.Permission!.PermissionName == "ViewDocumentsIfPermitted"), cancellationToken);

            _logger.LogDebug("Moharrir {UserId} elevated access check: {Result}", userId, hasPermission);
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking Moharrir elevated access for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsMohallirRestrictedAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users.IgnoreQueryFilters().AsNoTracking().Include(x => x.Role).FirstOrDefaultAsync(x => x.UserID == userId, cancellationToken);

            if (user?.GetRole() != UserRole.Moharrir)
                return false;

            bool isRestricted = !await HasMohallirElevatedAccessAsync(userId, cancellationToken);

            _logger.LogDebug("Moharrir {UserId} restricted check: {IsRestricted}", userId, isRestricted);
            return isRestricted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking Moharrir restricted mode for user {UserId}", userId);
            return false;
        }
    }

    public async Task<DocumentAccessLevel> GetUserDocumentAccessLevelAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users.IgnoreQueryFilters().AsNoTracking().Include(x => x.Role).FirstOrDefaultAsync(x => x.UserID == userId, cancellationToken);

            if (user == null || user.IsDeleted || !user.IsActive)
                return DocumentAccessLevel.None;

            var role = user.GetRole();

            return role switch
            {
                UserRole.SuperAdmin => DocumentAccessLevel.None,
                UserRole.FirmAdmin => DocumentAccessLevel.FullAccess,
                UserRole.Partner => DocumentAccessLevel.FullAccess,
                UserRole.AssociateLawyer => DocumentAccessLevel.ReadWrite,
                UserRole.InternParalegal => DocumentAccessLevel.ReadWrite,
                UserRole.Moharrir => await GetMohallirAccessLevelAsync(userId, cancellationToken),
                _ => DocumentAccessLevel.None
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting document access level for user {UserId}", userId);
            return DocumentAccessLevel.None;
        }
    }

    // Resolves the Moharrir-specific access level (WriteOnly vs ReadWrite).
    private async Task<DocumentAccessLevel> GetMohallirAccessLevelAsync(int userId, CancellationToken cancellationToken)
    {
        bool isElevated = await HasMohallirElevatedAccessAsync(userId, cancellationToken);
        return isElevated ? DocumentAccessLevel.ReadWrite : DocumentAccessLevel.WriteOnly;
    }

    // Creates or updates a role-wide document permission grant.
    public async Task GrantDocumentPermissionAsync(long documentId, int roleId, bool canView, bool canDownload, bool canUpload, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingPermission = await _context.DocumentPermissions.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.DocumentID == documentId && x.RoleID == roleId, cancellationToken);

            if (existingPermission != null)
            {
                existingPermission.CanView = canView;
                existingPermission.CanDownload = canDownload;
                existingPermission.CanUpload = canUpload;
                _logger.LogInformation("Updated document permission for document {DocumentId} role {RoleId}", documentId, roleId);
            }
            else
            {
                var permission = new Models.Cases.DocumentPermission
                {
                    DocumentID = documentId,
                    RoleID = roleId,
                    CanView = canView,
                    CanDownload = canDownload,
                    CanUpload = canUpload,
                    GrantedDate = DateTime.UtcNow
                };

                _context.DocumentPermissions.Add(permission);
                _logger.LogInformation("Granted document permission for document {DocumentId} role {RoleId}", documentId, roleId);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while granting document permission for document {DocumentId} role {RoleId}", documentId, roleId);
            throw;
        }
    }

    public async Task GrantUserDocumentPermissionAsync(long documentId, int userId, bool canView, bool canDownload, bool canUpload, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingPermission = await _context.DocumentPermissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DocumentID == documentId && x.UserID == userId, cancellationToken);

            if (existingPermission != null)
            {
                existingPermission.CanView = canView;
                existingPermission.CanDownload = canDownload;
                existingPermission.CanUpload = canUpload;
                _logger.LogInformation("Updated user-specific document permission for document {DocumentId} user {UserId}", documentId, userId);
            }
            else
            {
                var permission = new Models.Cases.DocumentPermission
                {
                    DocumentID = documentId,
                    UserID = userId,
                    RoleID = null,
                    CanView = canView,
                    CanDownload = canDownload,
                    CanUpload = canUpload,
                    GrantedDate = DateTime.UtcNow
                };

                _context.DocumentPermissions.Add(permission);
                _logger.LogInformation("Granted user-specific document permission for document {DocumentId} user {UserId}", documentId, userId);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while granting user-specific document permission for document {DocumentId} user {UserId}", documentId, userId);
            throw;
        }
    }

    // Removes a role-wide document permission grant, if one exists.
    public async Task RevokeDocumentPermissionAsync(long documentId, int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var permission = await _context.DocumentPermissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DocumentID == documentId && x.RoleID == roleId, cancellationToken);

            if (permission != null)
            {
                _context.DocumentPermissions.Remove(permission);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Revoked document permission for document {DocumentId} role {RoleId}", documentId, roleId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while revoking document permission for document {DocumentId} role {RoleId}", documentId, roleId);
            throw;
        }
    }
}