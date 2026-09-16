namespace LTSFrontend.Core.Enums
{
    /// <summary>
    /// Mirrors LTSBackend.Comman.Enum.RoleHierarchy. This is a client-side
    /// convenience only - it lets the UI grey out / hide role choices a user
    /// isn't allowed to assign before the request ever reaches the server,
    /// so the person gets instant feedback instead of a round-trip 403. The
    /// backend re-validates the exact same rule independently (see
    /// CreateUserCommandHandler / UpdateUserCommandHandler), so this class
    /// must never be trusted as the actual security boundary.
    /// </summary>
    public static class RoleHierarchy
    {
        /// <summary>
        /// True if <paramref name="actingUserRole"/> is allowed to assign
        /// <paramref name="targetRoleId"/> to another user. Lower numeric
        /// value = higher privilege (see UserRole), so a role may only ever
        /// assign a role that is STRICTLY numerically greater (i.e. lower
        /// privilege) than its own - never equal or higher. SuperAdmin can
        /// never be assigned through the normal user-management screens.
        /// </summary>
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

        /// <summary>
        /// Returns every role the given acting role is allowed to assign,
        /// for populating a "Role" dropdown on the Create/Edit User forms.
        /// </summary>
        public static IReadOnlyList<(int Id, string Name)> AssignableRolesFor(UserRole actingUserRole)
        {
            return UserRoleExtensions.All.Where(r =>
            {
                return CanAssignRole(actingUserRole, r.Id);
            }).ToList();
        }
    }
}
