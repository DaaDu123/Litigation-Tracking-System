namespace LTSFrontend.Core.Enums
{









    public static class RoleHierarchy
    {








        public static bool CanAssignRole(UserRole actingUserRole, int targetRoleId)
        {
            if (!System.Enum.IsDefined(typeof(UserRole), targetRoleId))
            {
                return false;
            }

            var targetRole = (UserRole)targetRoleId;

            if (targetRole == UserRole.SuperAdmin)
            {
                return false;
            }

            return (int)targetRole > (int)actingUserRole;
        }





        public static IReadOnlyList<(int Id, string Name)> AssignableRolesFor(UserRole actingUserRole)
        {
            return UserRoleExtensions.All.Where(r =>
            {
                return CanAssignRole(actingUserRole, r.Id);
            }).ToList();
        }
    }
}
