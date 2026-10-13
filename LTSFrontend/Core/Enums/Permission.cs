namespace LTSFrontend.Core.Enums
{









    public enum Permission
    {

        ManageFirms = 101,
        ViewSystemAuditLogs = 102,
        ManageDataMigration = 103,
        ManageSystemUsers = 104,


        ManageFirmUsers = 201,
        ViewFirmCaseDirectory = 202,
        AssignLawyersToCases = 203,
        ManageFirmSettings = 204,
        DeleteCases = 205,
        ViewLoginHistory = 206,
        DeleteLoginHistory = 207,
        ViewAuditLogs = 208,


        ViewFirmCases = 301,
        CreateCases = 302,
        UpdateCases = 303,
        AssignCases = 304,
        ViewAllDocuments = 305,
        DownloadDocuments = 306,
        ApproveFilings = 307,
        ViewFirmAnalytics = 308,


        ViewAssignedCases = 401,
        UploadDocuments = 402,
        DownloadAssignedDocuments = 403,
        AddCaseNotes = 404,
        TrackDeadlines = 405,
        LogBillableHours = 406,


        EnterCaseData = 501,
        UploadCaseDocuments = 502,
        ViewDocumentsIfPermitted = 503,
        DownloadDocumentsIfPermitted = 504,
        MaintainCaseRecords = 505,


        ViewDocumentsReadOnly = 601,
        DraftDocuments = 602,
        PerformResearch = 603,


        ViewDashboard = 701
    }






    public static class PermissionExtensions
    {



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


        public static IReadOnlyList<Permission> All { get; } =
            System.Enum.GetValues<Permission>();
    }
}
