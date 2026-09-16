namespace LTSFrontend.Core.Enums
{
    /// <summary>Mirrors LTSBackend.Comman.Enum.UserRole - keep IDs in sync.</summary>
    public enum UserRole
    {
        SuperAdmin = 1,
        FirmAdmin = 2,
        Partner = 3,
        AssociateLawyer = 4,
        Moharrir = 5,
        InternParalegal = 6
    }

    public static class UserRoleExtensions
    {
        public static IReadOnlyList<(int Id, string Name)> All { get; } = new List<(int, string)>
        {
            ((int)UserRole.SuperAdmin, "Super Admin"),
            ((int)UserRole.FirmAdmin, "Firm Admin"),
            ((int)UserRole.Partner, "Partner / Senior Lawyer"),
            ((int)UserRole.AssociateLawyer, "Associate Lawyer"),
            ((int)UserRole.Moharrir, "Moharrir"),
            ((int)UserRole.InternParalegal, "Intern / Paralegal")
        };

        public static string NameOf(int roleId)
        {
            return All.FirstOrDefault(r =>
            {
                return r.Id == roleId;
            }).Name ?? "Unknown";
        }

        // Roles a Firm Admin is allowed to assign when creating/editing a
        // user - mirrors the backend fix in RoleHierarchy.CanAssignRole
        // (strictly lower privilege only). SRS: "Firm Admin can create
        // only: Partner, Associate Lawyer, Moharrir, Intern/Paralegal" and
        // "Firm Admin cannot create another Firm Admin." Kept for backward
        // compatibility with existing call sites; equivalent to
        // AssignableBy((int)UserRole.FirmAdmin).
        public static IReadOnlyList<(int Id, string Name)> AssignableByFirmAdmin { get; } =
            All.Where(r =>
            {
                return r.Id > (int)UserRole.FirmAdmin;
            })
            .ToList();

        /// <summary>
        /// Generic version of AssignableByFirmAdmin - mirrors
        /// RoleHierarchy.CanAssignRole exactly: a user may only assign a
        /// role that is STRICTLY lower privilege (numerically greater) than
        /// their own. Needed now that Partner (not just FirmAdmin) can open
        /// UserFormModal to create/edit users - a Partner(3) should only see
        /// AssociateLawyer/Moharrir/InternParalegal (4,5,6) in the dropdown,
        /// never FirmAdmin or another Partner.
        /// </summary>
        public static IReadOnlyList<(int Id, string Name)> AssignableBy(int actingRoleId) =>
            All.Where(r => r.Id > actingRoleId).ToList();

        /// <summary>Convenience overload taking the acting user's role NAME
        /// (e.g. Session.Role) instead of its numeric ID. Returns an empty
        /// list if the name doesn't parse to a known UserRole.</summary>
        public static IReadOnlyList<(int Id, string Name)> AssignableBy(string? actingRoleName)
        {
            if (Enum.TryParse<UserRole>(actingRoleName, out var role))
                return AssignableBy((int)role);

            return Array.Empty<(int, string)>();
        }

        /// <summary>
        /// Maps the raw, compact role name the backend stores/returns (e.g.
        /// "AssociateLawyer", "InternParalegal", "FirmAdmin" - the exact
        /// string used for [Authorize(Roles=...)] matching, never spaced)
        /// to the friendly, spaced display name from the All list above
        /// (e.g. "Associate Lawyer", "Intern / Paralegal", "Firm Admin").
        /// Use this everywhere a role name from the API (UserDTO.RoleName,
        /// Session.Role, etc.) is shown to the user - never bind the raw
        /// value directly, or roles render un-spaced/without their full
        /// name (e.g. "Partner" instead of "Partner / Senior Lawyer").
        /// Falls back to the raw string unchanged if it doesn't match a
        /// known role, so an unexpected value still renders instead of
        /// disappearing.
        /// </summary>
        public static string DisplayName(string? rawRoleName)
        {
            if (string.IsNullOrWhiteSpace(rawRoleName))
                return rawRoleName ?? string.Empty;

            if (Enum.TryParse<UserRole>(rawRoleName, out var role))
                return NameOf((int)role);

            return rawRoleName;
        }
    }
}
