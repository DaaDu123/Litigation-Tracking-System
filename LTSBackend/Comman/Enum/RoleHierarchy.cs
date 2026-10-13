namespace LTSBackend.Comman.Enum;

public static class RoleHierarchy
{

    public static bool CanAssignRole(UserRole actingUserRole, int targetRoleId)
    {
        if (!System.Enum.IsDefined(typeof(UserRole), targetRoleId))
            return false;

        var targetRole = (UserRole)targetRoleId;



        if (targetRole == UserRole.SuperAdmin)
            return false;











        return (int)targetRole > (int)actingUserRole;
    }









    public static bool IsJoinableRole(int roleId)
    {
        if (!System.Enum.IsDefined(typeof(UserRole), roleId))
            return false;

        var role = (UserRole)roleId;

        return role is UserRole.Partner or UserRole.AssociateLawyer or UserRole.Moharrir or UserRole.InternParalegal;
    }
}