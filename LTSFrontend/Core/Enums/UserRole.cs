namespace LTSFrontend.Core.Enums
{

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








        public static IReadOnlyList<(int Id, string Name)> AssignableByFirmAdmin { get; } =
            All.Where(r =>
            {
                return r.Id > (int)UserRole.FirmAdmin;
            })
            .ToList();










        public static IReadOnlyList<(int Id, string Name)> AssignableBy(int actingRoleId) =>
            All.Where(r => r.Id > actingRoleId).ToList();




        public static IReadOnlyList<(int Id, string Name)> AssignableBy(string? actingRoleName)
        {
            if (Enum.TryParse<UserRole>(actingRoleName, out var role))
                return AssignableBy((int)role);

            return Array.Empty<(int, string)>();
        }















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
