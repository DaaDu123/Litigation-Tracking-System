namespace LTSFrontend.Core.Enums
{
    /// <summary>
    /// Mirrors LTSBackend.Comman.Enum.PermissionEnum - keep numeric values in
    /// sync with the backend at all times, since RolePermissions rows and the
    /// PermissionMatrix/AssignPermissions UI both key off these exact IDs.
    /// The 100-blocks group permissions by the role tier that typically holds
    /// them (100 = SuperAdmin, 200 = FirmAdmin, 300 = Partner, 400 = Associate
    /// Lawyer, 500 = Moharrir, 600 = Intern/Paralegal, 700 = cross-role), but
    /// actual grants live in RolePermissions - this enum only names the IDs.
    /// </summary>
    public enum Permission
    {
        // SUPER ADMIN
        ManageFirms = 101,
        ViewSystemAuditLogs = 102,
        ManageDataMigration = 103,
        ManageSystemUsers = 104,

        // FIRM ADMIN
        ManageFirmUsers = 201,
        ViewFirmCaseDirectory = 202,
        AssignLawyersToCases = 203,
        ManageFirmSettings = 204,
        DeleteCases = 205,
        ViewLoginHistory = 206,
        DeleteLoginHistory = 207,
        ViewAuditLogs = 208,

        // PARTNER / SENIOR LAWYER
        ViewFirmCases = 301,
        CreateCases = 302,
        UpdateCases = 303,
        AssignCases = 304,
        ViewAllDocuments = 305,
        DownloadDocuments = 306,
        ApproveFilings = 307,
        ViewFirmAnalytics = 308,

        // ASSOCIATE LAWYER
        ViewAssignedCases = 401,
        UploadDocuments = 402,
        DownloadAssignedDocuments = 403,
        AddCaseNotes = 404,
        TrackDeadlines = 405,
        LogBillableHours = 406,

        // MOHARRIR (LEGAL CLERK)
        EnterCaseData = 501,
        UploadCaseDocuments = 502,
        ViewDocumentsIfPermitted = 503,
        DownloadDocumentsIfPermitted = 504,
        MaintainCaseRecords = 505,

        // INTERN / PARALEGAL
        ViewDocumentsReadOnly = 601,
        DraftDocuments = 602,
        PerformResearch = 603,

        // CROSS-ROLE
        ViewDashboard = 701
    }

    /// <summary>
    /// Display helpers for <see cref="Permission"/> - used by the
    /// PermissionMatrix / AssignPermissions pages so checkbox grids can be
    /// grouped and labelled without hard-coding strings at every call site.
    /// </summary>
    public static class PermissionExtensions
    {
        /// <summary>
        /// Turns "ManageFirmUsers" into "Manage Firm Users" for display.
        /// </summary>
        public static string ToDisplayName(this Permission permission)
        {
            var name = permission.ToString();
            var chars = new List<char>(name.Length * 2);

            for (var i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                    chars.Add(' ');

                chars.Add(name[i]);
            }

            return new string(chars.ToArray());
        }

        /// <summary>
        /// Groups a permission under the role tier it was designed for, based
        /// on its numeric block (101-199 = SuperAdmin, 201-299 = FirmAdmin,
        /// etc.), purely for organizing the permission-matrix UI into
        /// sections - it does NOT restrict which role a permission can
        /// actually be assigned to (that's enforced server-side).
        /// </summary>
        public static string GroupName(this Permission permission)
        {
            var id = (int)permission;
            return id switch
            {
                >= 100 and < 200 => "Super Admin",
                >= 200 and < 300 => "Firm Admin",
                >= 300 and < 400 => "Partner / Senior Lawyer",
                >= 400 and < 500 => "Associate Lawyer",
                >= 500 and < 600 => "Moharrir",
                >= 600 and < 700 => "Intern / Paralegal",
                >= 700 and < 800 => "Cross-Role",
                _ => "Other"
            };
        }

        /// <summary>All defined permissions, for populating checkbox grids.</summary>
        public static IReadOnlyList<Permission> All { get; } =
            System.Enum.GetValues<Permission>();
    }
}
