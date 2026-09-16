namespace LTSFrontend.Core.Http
{
    /// <summary>
    /// Central place for every LTSBackend route the frontend calls.
    /// Keeps raw route strings out of the services/pages.
    /// </summary>
    public static class ApiEndpoints
    {
        private const string Base = "api";

        public static class Auth
        {
            private const string Root = Base + "/auth";
            public const string Register = Root + "/register";
            public const string VerifyOtp = Root + "/verify-otp";
            public const string ResendOtp = Root + "/resend-otp";
            public const string Login = Root + "/login";
            public const string Logout = Root + "/logout";
            public const string RefreshToken = Root + "/refresh-token";
            public const string ChangePassword = Root + "/change-password";
            public const string ForgotPassword = Root + "/forgot-password";
            public const string ResetPassword = Root + "/reset-password";
        }

        public static class Users
        {
            private const string Root = Base + "/users";
            public const string Base_ = Root;
            public const string MyProfile = Root + "/profile/me";
            public static string ById(int id) => $"{Root}/{id}";
            public static string Activate(int id) => $"{Root}/{id}/activate";
            public static string PermanentDelete(int id) => $"{Root}/{id}/permanent";
            public const string Deleted = Root + "/deleted";
            public static string Release(int id) => $"{Root}/{id}/release";
        }

        public static class Roles
        {
            private const string Root = Base + "/roles";
            public const string Base_ = Root;
            public static string ById(int id) => $"{Root}/{id}";
        }

        public static class Masters
        {
            public static class Courts
            {
                private const string Root = Base + "/courts";
                public const string Base_ = Root;
                public static string ById(int id) => $"{Root}/{id}";
            }

            public static class CaseCategories
            {
                private const string Root = Base + "/casecategories";
                public const string Base_ = Root;
                public static string ById(int id) => $"{Root}/{id}";
            }

            public static class CaseStages
            {
                private const string Root = Base + "/casestages";
                public const string Base_ = Root;
                public static string ById(int id) => $"{Root}/{id}";
            }

            public static class CaseStatuses
            {
                private const string Root = Base + "/casestatuses";
                public const string Base_ = Root;
                public static string ById(int id) => $"{Root}/{id}";
            }

            public static class Departments
            {
                private const string Root = Base + "/departments";
                public const string Base_ = Root;
                public static string ById(int id) => $"{Root}/{id}";
            }

            public static class DocumentTypes
            {
                private const string Root = Base + "/documenttypes";
                public const string Base_ = Root;
                public static string ById(int id) => $"{Root}/{id}";

                /// <summary>
                /// Lightweight {DocumentTypeID, TypeName} dropdown list, reachable by
                /// every role that can upload a document (Partner/AssociateLawyer/
                /// Moharrir/InternParalegal/FirmAdmin) — not just FirmAdmin/Partner
                /// like Base_ (the full master-data admin list). Use this to
                /// populate the "Document Type" dropdown on the upload form.
                /// </summary>
                public const string Options = Root + "/options";
            }
        }

        public static class Cases
        {
            private const string Root = Base + "/cases";
            public const string Base_ = Root;
            public static string ById(long id) => $"{Root}/{id}";
            public static string Status(long id) => $"{Root}/{id}/status";
            public static string StatusHistory(long id) => $"{Root}/{id}/status-history";
        }

        public static class CaseAssignments
        {
            private const string Root = Base + "/caseassignments";
            public const string Base_ = Root;
            public static string ByCase(long caseId) => $"{Root}/case/{caseId}";
            public const string MyCases = Root + "/my-cases";
            public static string ById(long id) => $"{Root}/{id}";
            public static string End(long id) => $"{Root}/{id}/end";
        }

        public static class CaseNotes
        {
            private const string Root = Base + "/casenotes";
            public const string Base_ = Root;
            public static string ByCase(long caseId) => $"{Root}/case/{caseId}";
            public static string ById(long id) => $"{Root}/{id}";
        }

        public static class CaseParties
        {
            public static string ByCase(long caseId) => $"{Base}/cases/{caseId}/parties";
            public static string ById(long caseId, long partyId) => $"{Base}/cases/{caseId}/parties/{partyId}";
        }

        public static class Hearings
        {
            private const string Root = Base + "/hearings";
            public const string Base_ = Root;
            public static string ById(long id) => $"{Root}/{id}";
            public const string Upcoming = Root + "/upcoming";
            public static string ByCase(long caseId) => $"{Root}/case/{caseId}";
            public static string Attendance(long hearingId) => $"{Root}/{hearingId}/attendance";
            public static string AttendanceById(long hearingId, long attendanceId) => $"{Root}/{hearingId}/attendance/{attendanceId}";
        }

        public static class Deadlines
        {
            private const string Root = Base + "/deadlines";
            public const string Base_ = Root;
            public static string ById(long id) => $"{Root}/{id}";
            public const string Upcoming = Root + "/upcoming";
            public static string ByCase(long caseId) => $"{Root}/case/{caseId}";
            public static string Complete(long id) => $"{Root}/{id}/complete";
        }

        public static class Milestones
        {
            private const string Root = Base + "/milestones";
            public const string Base_ = Root;
            public static string ById(long id) => $"{Root}/{id}";
            public static string ByCase(long caseId) => $"{Root}/case/{caseId}";
            public static string Complete(long id) => $"{Root}/{id}/complete";
        }

        public static class Documents
        {
            private const string Root = Base + "/documents";
            public const string Upload = Root + "/upload";
            public static string ById(long id) => $"{Root}/{id}";
            public static string ByCase(long caseId) => $"{Root}/case/{caseId}";
            public static string Download(long id) => $"{Root}/download/{id}";
            public static string Approve(long id) => $"{Root}/{id}/approve";
        }

        public static class AuditLogs
        {
            public const string Base_ = Base + "/auditlogs";
        }

        public static class LoginHistory
        {
            private const string Root = Base + "/loginhistory";
            public const string Base_ = Root;
            public const string My = Root + "/my";
        }

        public static class Firms
        {
            private const string Root = Base + "/firms";
            public const string Base_ = Root;
            public static string ById(int id) => $"{Root}/{id}";
            public static string Block(int id) => $"{Root}/{id}/block";
            public static string Unblock(int id) => $"{Root}/{id}/unblock";
            public static string Export(int id) => $"{Root}/{id}/export";
        }

        public static class FirmAdminRequests
        {
            private const string Root = Base + "/firmadminrequests";
            public const string Base_ = Root;
            public static string WithStatus(string? status) =>
                string.IsNullOrWhiteSpace(status) ? Root : $"{Root}?status={status}";
            public static string Approve(int id) => $"{Root}/{id}/approve";
            public static string Reject(int id) => $"{Root}/{id}/reject";
        }

        public static class ContactMessages
        {
            private const string Root = Base + "/contactmessages";
            public const string Base_ = Root;
        }

        public static class UserJoinRequests
        {
            private const string Root = Base + "/userjoinrequests";
            public const string Base_ = Root;
            public const string Firms = Root + "/firms";
            public static string WithStatus(string? status)
            {
                return string.IsNullOrWhiteSpace(status) ? Root : $"{Root}?status={status}";
            }

            public static string Approve(int id) => $"{Root}/{id}/approve";
            public static string Reject(int id) => $"{Root}/{id}/reject";
        }

        public static class Permissions
        {
            private const string Root = Base + "/permissions";
            public const string Base_ = Root;
            public const string Assign = Root + "/assign";
            public static string ByRoleId(int roleId) => $"{Root}/role/{roleId}";
        }

        public static class Profile
        {
            private const string Root = Base + "/profile";
            public const string Me = Root + "/me";
        }

        public static class Dashboard
        {
            public const string Stats = Base + "/dashboard";
        }

        public static class Notifications
        {
            private const string Root = Base + "/notifications";
            public const string Base_ = Root;
            public const string UnreadCount = Root + "/unread-count";
            public static string MarkAsRead(long id) => $"{Root}/{id}/read";
            public const string MarkAllAsRead = Root + "/read-all";
            public static string ById(long id) => $"{Root}/{id}";
        }
    }
}
