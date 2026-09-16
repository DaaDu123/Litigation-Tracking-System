using LTSBackend.Comman.Enum;

namespace LTSBackend.Services.Permissions;

public interface IPermissionService
{
    /// <summary>
    /// Checks whether the given user currently holds the named permission.
    /// Returns false (never throws) for unknown, inactive, or deleted users.
    /// </summary>
    Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every permission name granted to the user's role (or every
    /// permission that exists, for SuperAdmin).
    /// </summary>
    Task<List<string>> GetPermissionsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the given user is currently assigned the given role.
    /// </summary>
    Task<bool> HasRoleAsync(int userId, UserRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user's current role, or null if the user has no role,
    /// does not exist, or the stored RoleID is not a recognised UserRole.
    /// </summary>
    Task<UserRole?> GetUserRoleAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// SRS "View Firm Case Directory": true only for SuperAdmin, Firm Admin,
    /// and Senior Partner (i.e. holders of the ViewFirmCaseDirectory
    /// permission) - everyone else (Associate Lawyer, Moharrir, Intern) may
    /// only ever see cases they are individually assigned to, never the
    /// firm's full case list.
    /// </summary>
    Task<bool> HasFullCaseDirectoryVisibilityAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// SRS RBAC step "Case assignment (where applicable)": true only if the
    /// user has an active (non-ended) CaseAssignment row for this case, AND
    /// the case belongs to the user's own firm. SuperAdmin and users with
    /// full case-directory visibility (see above) are NOT automatically
    /// "assigned" by this check - callers that mean to allow those roles
    /// through regardless of assignment should check
    /// HasFullCaseDirectoryVisibilityAsync first and short-circuit.
    /// </summary>
    Task<bool> IsUserAssignedToCaseAsync(int userId, long caseId, CancellationToken cancellationToken = default);
}