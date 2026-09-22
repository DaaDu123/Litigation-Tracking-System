namespace LTSBackend.Comman.Enum;

public static class RoleHierarchy
{
    // Lower number = higher privilege (UserRole enum values already follow this)
    public static bool CanAssignRole(UserRole actingUserRole, int targetRoleId)
    {
        if (!System.Enum.IsDefined(typeof(UserRole), targetRoleId))
            return false;

        var targetRole = (UserRole)targetRoleId;

        // SuperAdmin is platform-only — nobody should be able to assign it
        // via the normal user-management endpoints.
        if (targetRole == UserRole.SuperAdmin)
            return false;

        // A user can only assign a role that is STRICTLY lower privilege
        // than their own (numerically greater), never equal or higher.
        // This is what enforces the SRS rule "Firm Admin cannot create
        // another Firm Admin" - the only callers of this method today are
        // CreateUserCommandHandler and ChangeUserRoleCommandHandler, both
        // FirmAdmin-only, and with a strict ">" a FirmAdmin(2) can no
        // longer assign FirmAdmin(2) to someone else - only
        // Partner/AssociateLawyer/Moharrir/InternParalegal (3,4,5,6),
        // exactly as the SRS's "Firm Admin can create only: Partner,
        // Associate Lawyer, Moharrir, Intern/Paralegal" states.
        return (int)targetRole > (int)actingUserRole;
    }

    // =====================================================
    // IS JOINABLE ROLE — used by the public UserJoinRequests flow
    // A person requesting to join an EXISTING firm (no acting user yet,
    // hence no CanAssignRole check is possible) may only ever request
    // Partner, AssociateLawyer, Moharrir, or InternParalegal - never
    // FirmAdmin (that's the separate FirmAdminRequests -> SuperAdmin
    // workflow) and never SuperAdmin (platform-only, never self-service).
    // =====================================================
    public static bool IsJoinableRole(int roleId)
    {
        if (!System.Enum.IsDefined(typeof(UserRole), roleId))
            return false;

        var role = (UserRole)roleId;

        return role is UserRole.Partner or UserRole.AssociateLawyer or UserRole.Moharrir or UserRole.InternParalegal;
    }
}