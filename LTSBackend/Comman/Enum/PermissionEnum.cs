namespace LTSBackend.Comman.Enum;

public enum PermissionEnum
{
    // SUPER ADMIN PERMISSIONS
    /// <summary>
    /// Create/Block/Remove Firms
    /// </summary>
    ManageFirms = 101,

    /// <summary>
    /// View system-wide audit logs
    /// </summary>
    ViewSystemAuditLogs = 102,

    /// <summary>
    /// Data migration and backup export
    /// </summary>
    ManageDataMigration = 103,

    /// <summary>
    /// Manage system-wide users
    /// </summary>
    ManageSystemUsers = 104,

    // FIRM ADMIN PERMISSIONS
    /// <summary>
    /// Create and manage firm users
    /// </summary>
    ManageFirmUsers = 201,

    /// <summary>
    /// View all firm cases (directory)
    /// </summary>
    ViewFirmCaseDirectory = 202,

    /// <summary>
    /// Assign/Remove lawyers to cases
    /// </summary>
    AssignLawyersToCases = 203,

    /// <summary>
    /// Manage firm settings and billing
    /// </summary>
    ManageFirmSettings = 204,

    /// <summary>
    /// Delete cases
    /// </summary>
    DeleteCases = 205,

    /// <summary>
    /// View this firm's own login history (BUG FIX: [HasPermission("ViewLoginHistory")]
    /// was already used on LoginHistoryController, but this permission never
    /// existed - meaning only SuperAdmin, who bypasses permission checks
    /// entirely, could ever view login history. Firm Admin is now granted
    /// this explicitly in SeedRolePermissions, scoped to their own firm only
    /// via the AppDbContext global query filter on LoginHistory.
    /// </summary>
    ViewLoginHistory = 206,

    /// <summary>
    /// Delete/cleanup this firm's own login history records. Deliberately
    /// NOT granted to Firm Admin in SeedRolePermissions (remains Super-Admin
    /// only) - login history is a security-relevant audit trail, and the
    /// SRS requires audit logs to be "immutable and tamper-resistant", so
    /// per-firm self-service deletion is intentionally not enabled by
    /// default. Grant this to FirmAdmin only if retention/GDPR requirements
    /// explicitly call for it.
    /// </summary>
    DeleteLoginHistory = 207,

    /// <summary>
    /// View this firm's own audit trail (AuditLogsController). BUG FIX:
    /// same class of gap as ViewLoginHistory above - [HasPermission("ViewAuditLogs")]
    /// existed on the controller but this permission was never seeded to
    /// any role, so only SuperAdmin (via the old blanket bypass) could ever
    /// view audit logs. GetAuditLogsHandler already scopes non-SuperAdmin
    /// callers to their own firm's users, so this is safe to grant to
    /// FirmAdmin (SRS §5.10.7 "System Administrator Interface" - Audit Logs
    /// panel - which per-firm maps to FirmAdmin, not the platform SuperAdmin).
    /// </summary>
    ViewAuditLogs = 208,

    // PARTNER / SENIOR LAWYER
    /// <summary>
    /// View firm-wide case portfolio
    /// </summary>
    ViewFirmCases = 301,

    /// <summary>
    /// Create new cases
    /// </summary>
    CreateCases = 302,

    /// <summary>
    /// Update case details
    /// </summary>
    UpdateCases = 303,

    /// <summary>
    /// Assign lawyers to cases
    /// </summary>
    AssignCases = 304,

    /// <summary>
    /// View all documents (firm-wide)
    /// </summary>
    ViewAllDocuments = 305,

    /// <summary>
    /// Download documents
    /// </summary>
    DownloadDocuments = 306,

    /// <summary>
    /// Approve critical filings
    /// </summary>
    ApproveFilings = 307,

    /// <summary>
    /// View firm analytics
    /// </summary>
    ViewFirmAnalytics = 308,

    // ASSOCIATE LAWYER
    /// <summary>
    /// View assigned cases only
    /// </summary>
    ViewAssignedCases = 401,

    /// <summary>
    /// Upload documents to cases
    /// </summary>
    UploadDocuments = 402,

    /// <summary>
    /// Download documents from assigned cases
    /// </summary>
    DownloadAssignedDocuments = 403,

    /// <summary>
    /// Add case notes and updates
    /// </summary>
    AddCaseNotes = 404,

    /// <summary>
    /// Track deadlines
    /// </summary>
    TrackDeadlines = 405,

    /// <summary>
    /// Log billable hours
    /// </summary>
    LogBillableHours = 406,

    // MOHARRIR (LEGAL CLERK)
    /// <summary>
    /// Data entry - court diaries, hearing dates
    /// </summary>
    EnterCaseData = 501,

    /// <summary>
    /// Upload documents (write-only for restricted)
    /// </summary>
    UploadCaseDocuments = 502,

    /// <summary>
    /// View documents (conditional - only if permission granted)
    /// </summary>
    ViewDocumentsIfPermitted = 503,

    /// <summary>
    /// Download documents (conditional - only if permission granted)
    /// </summary>
    DownloadDocumentsIfPermitted = 504,

    /// <summary>
    /// Maintain case records
    /// </summary>
    MaintainCaseRecords = 505,

    // INTERN / PARALEGAL
    /// <summary>
    /// View documents read-only
    /// </summary>
    ViewDocumentsReadOnly = 601,

    /// <summary>
    /// Draft documents (create, not publish)
    /// </summary>
    DraftDocuments = 602,

    /// <summary>
    /// Assist with research
    /// </summary>
    PerformResearch = 603,

    // CROSS-ROLE (every authenticated role has its own dashboard)
    /// <summary>
    /// View one's own role-scoped dashboard (DashboardController). BUG FIX:
    /// same gap as ViewLoginHistory/ViewAuditLogs above - this permission
    /// was never seeded to any role, so only SuperAdmin (via the old
    /// blanket bypass) could ever load a dashboard. Every role gets this -
    /// GetSuperAdminDashboardHandler / GetFirmDashboardHandler each return a
    /// different, role-appropriate DTO shape, so granting the permission
    /// broadly does NOT mean everyone sees the same data (Roles SRS: "no
    /// one can use or view another role's dashboard").
    /// </summary>
    ViewDashboard = 701
}