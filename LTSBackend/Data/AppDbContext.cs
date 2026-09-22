using System.Security.Claims;
using LTSBackend.Models.Audit;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Masters;
using LTSBackend.Models.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UserRole = LTSBackend.Comman.Enum.UserRole;
using LTSBackend.Comman.Enum;

namespace LTSBackend.Data;

/// <summary>
/// AppDbContext: Main Database Context with Complete Seed Data.
///
/// Contains all DbSets, relationships, constraints, global multi-tenant
/// query filters, and complete seed data for production-ready
/// initialization.
///
/// TENANT ISOLATION (SRS "Multi-Tenant Security" / "Row-Level Security"):
/// every tenant-owned table is given a global query filter below so that
/// EVERY query issued through this context - in every handler, in every
/// feature slice, present or future - is automatically scoped to the
/// calling user's own firm. This is enforced here, once, instead of
/// relying on every single handler remembering to add its own
/// ".Where(x => x.FirmID == ...)" - a single missed filter in any one
/// handler would otherwise be a full cross-tenant data leak (IDOR).
/// Handlers that legitimately need to bypass this (e.g. this class's own
/// authorization services) call ".IgnoreQueryFilters()" explicitly.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // The identity of the caller making the current request, if any. Null
    // outside of an HTTP request (migrations, seeding, background services).
    private ClaimsPrincipal? RequestUser => _httpContextAccessor?.HttpContext?.User;

    private bool IsAuthenticatedRequest => RequestUser?.Identity?.IsAuthenticated == true;

    private bool IsSuperAdminRequest => RequestUser?.FindFirstValue(ClaimTypes.Role) == RoleNames.SuperAdmin;

    // The acting user's FirmID claim, parsed from the current JWT. Null for
    // SuperAdmin (who has no firm) and for requests with no FirmID claim.
    private int? RequestFirmId
    {
        get
        {
            var value = RequestUser?.FindFirstValue("FirmID");
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    /// <summary>
    /// True when tenant scoping must be skipped entirely: there is no HTTP
    /// request in flight (migrations/seeding/background jobs), the caller
    /// is not authenticated yet (e.g. login/forgot-password looking a user
    /// up by email before any FirmID is known), or the caller is the
    /// platform-wide Super Admin, who is explicitly allowed to see every
    /// tenant (SRS "Super Admin can access every tenant. No other role can.").
    /// </summary>
    private bool BypassTenantFilter => !IsAuthenticatedRequest || IsSuperAdminRequest;

    // SECURITY MODELS (User, Role, Permission)
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Firm> Firms { get; set; } = null!;
    public DbSet<FirmAdminRequest> FirmAdminRequests { get; set; } = null!;
    public DbSet<UserJoinRequest> UserJoinRequests { get; set; } = null!;
    public DbSet<FirmMembershipEvent> FirmMembershipEvents { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<NotificationType> NotificationTypes { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<UserOtp> UserOtps { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<LoginHistory> LoginHistories { get; set; } = null!;

    // AUDIT MODELS
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    // MASTER TABLES (Court, Category, Status, Stage, etc.)
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Court> Courts { get; set; } = null!;
    public DbSet<CaseCategory> CaseCategories { get; set; } = null!;
    public DbSet<CaseStatus> CaseStatuses { get; set; } = null!;
    public DbSet<CaseStage> CaseStages { get; set; } = null!;
    public DbSet<DocumentType> DocumentTypes { get; set; } = null!;

    // CORE CASE MANAGEMENT (Cases, Parties, Assignments)
    public DbSet<Case> Cases { get; set; } = null!;
    public DbSet<CaseParty> CaseParties { get; set; } = null!;
    public DbSet<CaseAssignment> CaseAssignments { get; set; } = null!;
    public DbSet<CaseStatusHistory> CaseStatusHistories { get; set; } = null!;
    public DbSet<CaseMilestone> CaseMilestones { get; set; } = null!;

    // HEARINGS & DEADLINES
    public DbSet<Hearing> Hearings { get; set; } = null!;
    public DbSet<HearingAttendance> HearingAttendances { get; set; } = null!;
    public DbSet<Deadline> Deadlines { get; set; } = null!;

    // DOCUMENTS & NOTES
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentPermission> DocumentPermissions { get; set; } = null!;
    public DbSet<CaseNote> CaseNotes { get; set; } = null!;

    // NOTIFICATIONS
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USER ENTITY CONFIGURATION
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserID);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique().HasFilter("[IsDeleted] = 0");
            // Phone/CNIC are always normalized (PakistaniFormat) before
            // being written, so these indexes are the real database-level
            // backstop for uniqueness (never relying on FluentValidation
            // alone - see spec §30). Filtered so multiple NULLs (not yet
            // set) and soft-deleted rows never collide.
            entity.HasIndex(e => e.Phone).IsUnique().HasDatabaseName("IX_Users_Phone").HasFilter("[Phone] IS NOT NULL AND [IsDeleted] = 0");
            entity.HasIndex(e => e.CNIC).IsUnique().HasDatabaseName("IX_Users_CNIC").HasFilter("[CNIC] IS NOT NULL AND [IsDeleted] = 0");
            entity.HasOne(e => e.Firm).WithMany(f => f.Users).HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.FirmID);
            entity.HasMany(e => e.RefreshTokens).WithOne(r => r.User).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.UserOtps).WithOne(o => o.User).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.PasswordResetTokens).WithOne(t => t.User).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.LoginHistories).WithOne(l => l.User).OnDelete(DeleteBehavior.Cascade);

            // Global filter: hide soft-deleted users, and restrict every
            // query to the caller's own firm unless bypassed (SuperAdmin /
            // background job / not-yet-authenticated login lookup).
            entity.HasQueryFilter(e => !e.IsDeleted && (BypassTenantFilter || e.FirmID == RequestFirmId));
        });

        // FIRM ENTITY CONFIGURATION (multi-tenant workspace)
        modelBuilder.Entity<Firm>(entity =>
        {
            entity.HasKey(e => e.FirmID);
            entity.Property(e => e.FirmName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.FirmCode).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.FirmCode).IsUnique();
            entity.Property(e => e.MigrationStatus).IsRequired().HasMaxLength(30).HasDefaultValue("None");
            entity.Property(e => e.MigrationNotes).HasMaxLength(500);
        });

        // FIRM ADMIN REQUEST ENTITY CONFIGURATION
        // (public "request to become a Firm Admin" -> SuperAdmin approval)
        modelBuilder.Entity<FirmAdminRequest>(entity =>
        {
            entity.HasKey(e => e.RequestID);
            // FirmName/FirmCode/AdminFullName are no longer collected at
            // submission (registration is Email+Password only) - system
            // generates placeholders on Approve, so these stay optional here.
            entity.Property(e => e.FirmName).HasMaxLength(150);
            entity.Property(e => e.FirmCode).HasMaxLength(30);
            entity.Property(e => e.AdminFullName).HasMaxLength(150);
            entity.Property(e => e.AdminEmail).IsRequired().HasMaxLength(150);
            entity.Property(e => e.AdminPasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            entity.HasIndex(e => e.FirmCode);
            entity.HasIndex(e => e.AdminEmail);
            entity.HasIndex(e => e.Status);
        });

        // USER JOIN REQUEST ENTITY CONFIGURATION
        // (public "request to join an existing firm" as Partner/Associate/
        // Moharrir/Intern -> that firm's FirmAdmin approval). Tenant-scoped
        // via the same query-filter pattern as User below: an authenticated
        // FirmAdmin only ever sees requests aimed at their own FirmID,
        // while an anonymous submitter (BypassTenantFilter == true, since
        // IsAuthenticatedRequest is false) can still hit the uniqueness
        // checks in SubmitUserJoinRequestCommandHandler across every firm.
        modelBuilder.Entity<UserJoinRequest>(entity =>
        {
            entity.HasKey(e => e.RequestID);
            // FullName/Email/PasswordHash are legacy/display-only now - the
            // requester already has an account (see UserID) by submission
            // time in the new flow, so these are no longer required here.
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.FirmID);
            entity.HasIndex(e => e.UserID);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == RequestFirmId);
        });

        // FIRM MEMBERSHIP EVENT ENTITY CONFIGURATION (block/unblock/remove
        // history). Tenant-scoped the same way as UserJoinRequest - a
        // FirmAdmin only ever sees events for their own FirmID.
        modelBuilder.Entity<FirmMembershipEvent>(entity =>
        {
            entity.HasKey(e => e.EventID);
            entity.Property(e => e.ActionType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.UserID, e.FirmID });
            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == RequestFirmId);
        });

        // ROLE ENTITY CONFIGURATION
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleID);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.RoleName).IsUnique();
            entity.HasMany(e => e.RolePermissions).WithOne(rp => rp.Role).OnDelete(DeleteBehavior.Cascade);
        });

        // PERMISSION ENTITY CONFIGURATION
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionID);
            entity.Property(e => e.PermissionName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.PermissionName).IsUnique();
        });

        // ROLEPERMISSION ENTITY CONFIGURATION (join table)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(x => x.RolePermissionID);
            entity.HasIndex(x => new { x.RoleID, x.PermissionID }).IsUnique();
            entity.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionID).OnDelete(DeleteBehavior.Cascade);
        });

        // REFRESHTOKEN ENTITY CONFIGURATION
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenID);

            entity.Property(e => e.Token).IsRequired().HasMaxLength(64);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => new { e.UserID, e.IsRevoked });

            entity.Property(e => e.ExpiryDate).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade);

            // Cascading tenant filter via the owning User's FirmID. Harmless
            // during refresh-token exchange itself (that request isn't
            // authenticated via the access token, so BypassTenantFilter is
            // true), but protects any future admin-facing "list tokens" view.
            entity.HasQueryFilter(e => BypassTenantFilter || e.User.FirmID == RequestFirmId);
        });

        // USEROTP ENTITY CONFIGURATION
        modelBuilder.Entity<UserOtp>(entity =>
        {
            entity.HasKey(e => e.OtpID);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.OtpCode).IsRequired().HasMaxLength(6);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.UserOtps).HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            entity.HasIndex(e => new { e.Email, e.OtpCode });

            // Cascading tenant filter via the (optional) owning User's FirmID.
            // OTP flows run before authentication (registration/forgot
            // password), so BypassTenantFilter is true there and this filter
            // only matters for any future authenticated OTP management view.
            entity.HasQueryFilter(e => BypassTenantFilter || e.User == null || e.User.FirmID == RequestFirmId);
        });

        // PASSWORDRESETTOKEN ENTITY CONFIGURATION
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenID);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.PasswordResetTokens).HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade).IsRequired();
            // TokenHash is looked up directly (never Email+code), and must be unique
            // so a hash collision can never resolve to more than one live token.
            entity.HasIndex(e => e.TokenHash).IsUnique();

            // Same reasoning as UserOtp: this flow runs before authentication
            // (BypassTenantFilter is true there), the filter only matters for
            // any future authenticated admin view over these tokens.
            entity.HasQueryFilter(e => BypassTenantFilter || e.User == null || e.User!.FirmID == RequestFirmId);
        });

        // LOGINHISTORY ENTITY CONFIGURATION
        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.LoginID);
            entity.Property(e => e.UserID).IsRequired();
            entity.Property(e => e.LoginTime).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.LoginHistories).HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserID, e.LoginTime }).IsDescending(false, true);

            // Cascading tenant filter via the owning User's FirmID. This is
            // the fix for a confirmed cross-tenant leak: GetAllLoginHistory
            // previously returned every firm's login IPs/emails to any Firm
            // Admin with the ViewLoginHistory permission. With this filter,
            // that same handler is now automatically scoped to one firm.
            entity.HasQueryFilter(e => BypassTenantFilter || e.User.FirmID == RequestFirmId);
        });

        // AUDITLOG ENTITY CONFIGURATION
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogID);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasIndex(e => e.Timestamp).IsDescending();
            entity.HasIndex(e => e.UserID);


            entity.HasQueryFilter(e => BypassTenantFilter || (e.User != null && e.User.FirmID == RequestFirmId));
        });

        // COURT ENTITY CONFIGURATION (per-tenant scoping added - see
        // Models/Masters/Court.cs for the full rationale)
        modelBuilder.Entity<Court>(entity =>
        {
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);

            // A caller sees system-wide global courts (FirmID null) plus
            // their own firm's custom courts. SuperAdmin bypasses and sees
            // every court, including every other firm's custom entries.
            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == null || e.FirmID == RequestFirmId);
        });

        // DEPARTMENT ENTITY CONFIGURATION (per-tenant scoping added - see
        // Models/Masters/Department.cs for the full rationale)
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == null || e.FirmID == RequestFirmId);
        });

        // CASECATEGORY / CASESTATUS / CASESTAGE / DOCUMENTTYPE ENTITY
        // CONFIGURATION (per-tenant scoping - same pattern as Court/Department)
        modelBuilder.Entity<CaseCategory>(entity =>
        {
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == null || e.FirmID == RequestFirmId);
        });

        modelBuilder.Entity<CaseStatus>(entity =>
        {
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == null || e.FirmID == RequestFirmId);
        });

        modelBuilder.Entity<CaseStage>(entity =>
        {
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == null || e.FirmID == RequestFirmId);
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == null || e.FirmID == RequestFirmId);
        });

        // CASE ENTITY CONFIGURATION
        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.CaseID);
            entity.Property(e => e.InternalReferenceNo).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.InternalReferenceNo).IsUnique();
            entity.Property(e => e.CaseNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CaseTitle).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Priority).IsRequired().HasMaxLength(20);
            entity.Property(e => e.SubjectMatter).IsRequired().HasMaxLength(255);

            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Court).WithMany().HasForeignKey(e => e.CourtID).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryID).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Status).WithMany().HasForeignKey(e => e.StatusID).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageID).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.ResponsibleDepartmentID).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.LegalOfficer).WithMany().HasForeignKey(e => e.CurrentLegalOfficerID).OnDelete(DeleteBehavior.Restrict);

            // Performance indexes
            entity.HasIndex(e => e.StatusID);
            entity.HasIndex(e => e.CourtID);
            entity.HasIndex(e => e.CategoryID);
            entity.HasIndex(e => e.CaseNumber);
            entity.HasIndex(e => e.FirmID);

            // Global tenant filter: a Case belongs to exactly one firm.
            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == RequestFirmId);
        });

        // CASE PARTIES ENTITY CONFIGURATION
        modelBuilder.Entity<CaseParty>(entity =>
        {
            entity.HasKey(e => e.PartyID);
            entity.Property(e => e.PartyType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PartyName).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Case).WithMany(c => c.CaseParties).HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CaseID);

            // Cascading tenant filter via the owning Case's FirmID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });

        // CASE ASSIGNMENTS ENTITY CONFIGURATION
        modelBuilder.Entity<CaseAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentID);
            entity.Property(e => e.AssignmentType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.LeadCounsel).HasColumnName("IsLeadCounsel");
            entity.HasOne(e => e.Case).WithMany(c => c.CaseAssignments).HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UserID);
            entity.HasIndex(e => new { e.CaseID, e.EndDate });

            // Cascading tenant filter via the owning Case's FirmID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });

        // CASE STATUS HISTORY ENTITY CONFIGURATION
        modelBuilder.Entity<CaseStatusHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryID);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);

            // Cascading tenant filter via the owning Case's FirmID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });

        // CASE MILESTONES ENTITY CONFIGURATION
        modelBuilder.Entity<CaseMilestone>(entity =>
        {
            entity.HasKey(e => e.MilestoneID);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CaseID);

            // Cascading tenant filter via the owning Case's FirmID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });

        // HEARINGS ENTITY CONFIGURATION
        modelBuilder.Entity<Hearing>(entity =>
        {
            entity.HasKey(e => e.HearingID);
            entity.HasOne(e => e.Case).WithMany(c => c.Hearings).HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Court).WithMany().HasForeignKey(e => e.CourtID).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.HearingAttendances).WithOne(ha => ha.Hearing).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.HearingDate);
            entity.HasIndex(e => e.CaseID);

            // Cascading tenant filter via the owning Case's FirmID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });

        // HEARING ATTENDANCE ENTITY CONFIGURATION
        modelBuilder.Entity<HearingAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceID);
            entity.HasOne(e => e.Hearing).WithMany(h => h.HearingAttendances).HasForeignKey(e => e.HearingID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.HearingID, e.UserID }).IsUnique();

            // Cascading tenant filter via Hearing -> Case's FirmID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Hearing.Case.FirmID == RequestFirmId);
        });

        // DEADLINES ENTITY CONFIGURATION
        modelBuilder.Entity<Deadline>(entity =>
        {
            entity.HasKey(e => e.DeadlineID);
            entity.HasOne(e => e.Case).WithMany(c => c.Deadlines).HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => new { e.CaseID, e.Completed });

            // Cascading tenant filter via the owning Case's FirmID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });

        // DOCUMENTS ENTITY CONFIGURATION
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentID);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.DocumentType).WithMany().HasForeignKey(e => e.DocumentTypeID).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.DocumentPermissions).WithOne(dp => dp.Document).OnDelete(DeleteBehavior.Cascade);

            // Cascading tenant filter via the owning Case's FirmID. This is
            // the single most important filter in the file: it is what
            // stops a Moharrir/Lawyer/FirmAdmin from ever retrieving another
            // firm's document just by guessing/incrementing a DocumentID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });

        // DOCUMENT PERMISSIONS ENTITY CONFIGURATION
        modelBuilder.Entity<DocumentPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionID);
            entity.HasOne(e => e.Document).WithMany(d => d.DocumentPermissions).HasForeignKey(e => e.DocumentID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany().HasForeignKey(e => e.RoleID).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.DocumentID, e.RoleID });
            entity.HasIndex(e => new { e.DocumentID, e.UserID });

            // Cascading tenant filter via Document -> Case's FirmID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Document.Case.FirmID == RequestFirmId);
        });

        // CASE NOTES ENTITY CONFIGURATION
        modelBuilder.Entity<CaseNote>(entity =>
        {
            entity.HasKey(e => e.NoteID);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.CaseID);

            // Cascading tenant filter via the owning Case's FirmID.
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });
        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.NotificationTypeID);
            entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.TypeName).IsUnique();
        });

        // NOTIFICATIONS ENTITY CONFIGURATION
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationID);
            entity.HasOne(e => e.NotificationType).WithMany(t => t.Notifications).HasForeignKey(e => e.NotificationTypeID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.Priority).HasMaxLength(20);

            // Cascading tenant filter via the notified User's FirmID (every
            // Notification always has a UserID; CaseID is optional so the
            // Case navigation alone isn't reliable for this filter).
            entity.HasQueryFilter(e => BypassTenantFilter || (e.User != null && e.User.FirmID == RequestFirmId));
        });

        // Seed data - MINIMAL bootstrap set only.
        // Firms, Departments, Courts, CaseCategories, CaseStatus, CaseStages,
        // and DocumentTypes are intentionally NOT seeded. Nothing in the
        // codebase references their IDs by hardcoded number (verified), so
        // the app works fine without them - Firms are created organically via
        // the Request-Firm-Admin-Access approval flow, and the master data
        // tables can be populated later by a SuperAdmin from inside the app.
        //
        // Roles + Permissions + RolePermissions + NotificationTypes stay
        // seeded because they ARE relied on by hardcoded IDs elsewhere in the
        // code (CreateUserCommandHandler, SubmitFirmAdminRequestCommandHandler,
        // SubmitUserJoinRequestCommandHandler, AssignCaseHandler, ReminderService,
        // FirmAdminRequest approval) - removing them would crash those flows.
        SeedRoles(modelBuilder);
        SeedPermissions(modelBuilder);
        SeedRolePermissions(modelBuilder);
        SeedUsers(modelBuilder);
        SeedNotificationTypes(modelBuilder);
    }

    // NOTE: Seed data (HasData) must be deterministic. Using DateTime.UtcNow here
    // (or as a property's default initializer) bakes a different value into the
    // model every single time the project is built, which makes EF Core think the
    // model has "pending changes" forever, even with no real schema change.
    // A fixed constant keeps the seeded rows stable across builds/migrations.
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // USERS - Bootstrap SuperAdmin ONLY.
    // Every other account (FirmAdmin, Partner, AssociateLawyer, Moharrir,
    // InternParalegal) is created organically at runtime:
    //   - FirmAdmin comes from the "Request Firm Admin Access" flow, once a
    //     SuperAdmin approves it (a new Firm is created at that point too).
    //   - Partner/Associate/Moharrir/Intern are added afterwards by that
    //     FirmAdmin via "Create User".
    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            // SuperAdmin - Platform Owner (bootstrap account)
            new User
            {
                UserID = 1,
                Email = "saadmuhammad19115@gmail.com",
                FullName = "Super Administrator",
                PasswordHash = "$2b$12$AGEF6nAJGVAKB/AtUDhyFuq23a7GuZLpG4g7dUeYvYPXmEimSSXN6",
                FirmID = null,
                RoleID = (int)UserRole.SuperAdmin,
                Designation = "System Administrator",
                IsExternal = false,
                IsActive = true,
                SecurityStamp = "SEED-STAMP-USER-0001",
                CreatedAt = SeedTimestamp
            }
        );
    }

    // ROLES - 6 role levels in hierarchy
    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleID = (int)UserRole.SuperAdmin, RoleName = nameof(UserRole.SuperAdmin), Description = "System-wide management and data custody", IsSystemRole = true, IsActive = true },
            new Role { RoleID = (int)UserRole.FirmAdmin, RoleName = nameof(UserRole.FirmAdmin), Description = "Workspace owner - manages specific law firm", IsSystemRole = false, IsActive = true },
            new Role { RoleID = (int)UserRole.Partner, RoleName = nameof(UserRole.Partner), Description = "Senior lawyer - supervises case teams", IsSystemRole = false, IsActive = true },
            new Role { RoleID = (int)UserRole.AssociateLawyer, RoleName = nameof(UserRole.AssociateLawyer), Description = "Day-to-day legal work", IsSystemRole = false, IsActive = true },
            new Role { RoleID = (int)UserRole.Moharrir, RoleName = nameof(UserRole.Moharrir), Description = "Legal clerk / Data entry operator", IsSystemRole = false, IsActive = true },
            new Role { RoleID = (int)UserRole.InternParalegal, RoleName = nameof(UserRole.InternParalegal), Description = "Temporary staff / Junior assistant", IsSystemRole = false, IsActive = true }
        );
    }

    // PERMISSIONS - 30+ granular permissions
    private static void SeedPermissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasData(
            // Super Admin Permissions
            new Permission { PermissionID = (int)PermissionEnum.ManageFirms, PermissionName = nameof(PermissionEnum.ManageFirms), Description = "Create, block, remove firms" },
            new Permission { PermissionID = (int)PermissionEnum.ViewSystemAuditLogs, PermissionName = nameof(PermissionEnum.ViewSystemAuditLogs), Description = "View system-wide audit logs" },
            new Permission { PermissionID = (int)PermissionEnum.ManageDataMigration, PermissionName = nameof(PermissionEnum.ManageDataMigration), Description = "Manage firm data migration" },
            new Permission { PermissionID = (int)PermissionEnum.ManageSystemUsers, PermissionName = nameof(PermissionEnum.ManageSystemUsers), Description = "Manage all system users" },

            // Firm Admin Permissions
            new Permission { PermissionID = (int)PermissionEnum.ManageFirmUsers, PermissionName = nameof(PermissionEnum.ManageFirmUsers), Description = "Create and manage firm users" },
            new Permission { PermissionID = (int)PermissionEnum.ViewFirmCaseDirectory, PermissionName = nameof(PermissionEnum.ViewFirmCaseDirectory), Description = "View all cases in firm" },
            new Permission { PermissionID = (int)PermissionEnum.AssignLawyersToCases, PermissionName = nameof(PermissionEnum.AssignLawyersToCases), Description = "Assign lawyers to cases" },
            new Permission { PermissionID = (int)PermissionEnum.ManageFirmSettings, PermissionName = nameof(PermissionEnum.ManageFirmSettings), Description = "Manage firm settings and billing" },
            new Permission { PermissionID = (int)PermissionEnum.DeleteCases, PermissionName = nameof(PermissionEnum.DeleteCases), Description = "Delete cases" },
            new Permission { PermissionID = (int)PermissionEnum.ViewLoginHistory, PermissionName = nameof(PermissionEnum.ViewLoginHistory), Description = "View this firm's own login history" },
            new Permission { PermissionID = (int)PermissionEnum.DeleteLoginHistory, PermissionName = nameof(PermissionEnum.DeleteLoginHistory), Description = "Delete/cleanup this firm's own login history records" },
            new Permission { PermissionID = (int)PermissionEnum.ViewAuditLogs, PermissionName = nameof(PermissionEnum.ViewAuditLogs), Description = "View this firm's own audit trail" },
            new Permission { PermissionID = (int)PermissionEnum.UploadDocuments, PermissionName = nameof(PermissionEnum.UploadDocuments), Description = "Upload documents" },

            // Partner/Senior Lawyer Permissions
            new Permission { PermissionID = (int)PermissionEnum.ViewFirmCases, PermissionName = nameof(PermissionEnum.ViewFirmCases), Description = "View firm cases" },
            new Permission { PermissionID = (int)PermissionEnum.CreateCases, PermissionName = nameof(PermissionEnum.CreateCases), Description = "Create new cases" },
            new Permission { PermissionID = (int)PermissionEnum.UpdateCases, PermissionName = nameof(PermissionEnum.UpdateCases), Description = "Update case information" },
            new Permission { PermissionID = (int)PermissionEnum.AssignCases, PermissionName = nameof(PermissionEnum.AssignCases), Description = "Assign cases to lawyers" },
            new Permission { PermissionID = (int)PermissionEnum.ViewAllDocuments, PermissionName = nameof(PermissionEnum.ViewAllDocuments), Description = "View all case documents" },
            new Permission { PermissionID = (int)PermissionEnum.DownloadDocuments, PermissionName = nameof(PermissionEnum.DownloadDocuments), Description = "Download documents" },
            new Permission { PermissionID = (int)PermissionEnum.ApproveFilings, PermissionName = nameof(PermissionEnum.ApproveFilings), Description = "Approve critical filings" },
            new Permission { PermissionID = (int)PermissionEnum.ViewFirmAnalytics, PermissionName = nameof(PermissionEnum.ViewFirmAnalytics), Description = "View firm analytics and reports" },

            // Associate Lawyer Permissions
            new Permission { PermissionID = (int)PermissionEnum.ViewAssignedCases, PermissionName = nameof(PermissionEnum.ViewAssignedCases), Description = "View assigned cases only" },
            new Permission { PermissionID = (int)PermissionEnum.DownloadAssignedDocuments, PermissionName = nameof(PermissionEnum.DownloadAssignedDocuments), Description = "Download assigned case documents" },
            new Permission { PermissionID = (int)PermissionEnum.AddCaseNotes, PermissionName = nameof(PermissionEnum.AddCaseNotes), Description = "Add notes to cases" },
            new Permission { PermissionID = (int)PermissionEnum.TrackDeadlines, PermissionName = nameof(PermissionEnum.TrackDeadlines), Description = "Track case deadlines" },
            new Permission { PermissionID = (int)PermissionEnum.LogBillableHours, PermissionName = nameof(PermissionEnum.LogBillableHours), Description = "Log billable hours" },

            // Moharrir Permissions
            new Permission { PermissionID = (int)PermissionEnum.EnterCaseData, PermissionName = nameof(PermissionEnum.EnterCaseData), Description = "Enter case data" },
            new Permission { PermissionID = (int)PermissionEnum.UploadCaseDocuments, PermissionName = nameof(PermissionEnum.UploadCaseDocuments), Description = "Upload case documents" },
            new Permission { PermissionID = (int)PermissionEnum.ViewDocumentsIfPermitted, PermissionName = nameof(PermissionEnum.ViewDocumentsIfPermitted), Description = "View documents if permitted" },
            new Permission { PermissionID = (int)PermissionEnum.DownloadDocumentsIfPermitted, PermissionName = nameof(PermissionEnum.DownloadDocumentsIfPermitted), Description = "Download documents if permitted" },
            new Permission { PermissionID = (int)PermissionEnum.MaintainCaseRecords, PermissionName = nameof(PermissionEnum.MaintainCaseRecords), Description = "Maintain case records" },

            // Intern/Paralegal Permissions
            new Permission { PermissionID = (int)PermissionEnum.ViewDocumentsReadOnly, PermissionName = nameof(PermissionEnum.ViewDocumentsReadOnly), Description = "View documents (read-only)" },
            new Permission { PermissionID = (int)PermissionEnum.DraftDocuments, PermissionName = nameof(PermissionEnum.DraftDocuments), Description = "Draft legal documents" },
            new Permission { PermissionID = (int)PermissionEnum.PerformResearch, PermissionName = nameof(PermissionEnum.PerformResearch), Description = "Perform legal research" },

            // Cross-role: every role has its own dashboard
            new Permission { PermissionID = (int)PermissionEnum.ViewDashboard, PermissionName = nameof(PermissionEnum.ViewDashboard), Description = "View one's own role-scoped dashboard" },

            // Firm/User workflow: block, remove, role change, Firm Admin availability
            new Permission { PermissionID = (int)PermissionEnum.BlockFirmUser, PermissionName = nameof(PermissionEnum.BlockFirmUser), Description = "Block a firm user" },
            new Permission { PermissionID = (int)PermissionEnum.UnblockFirmUser, PermissionName = nameof(PermissionEnum.UnblockFirmUser), Description = "Unblock a previously blocked firm user" },
            new Permission { PermissionID = (int)PermissionEnum.RemoveFirmUser, PermissionName = nameof(PermissionEnum.RemoveFirmUser), Description = "Remove a user from the firm" },
            new Permission { PermissionID = (int)PermissionEnum.ChangeFirmUserRole, PermissionName = nameof(PermissionEnum.ChangeFirmUserRole), Description = "Change a firm user's role" },
            new Permission { PermissionID = (int)PermissionEnum.ManageFirmAdminAvailability, PermissionName = nameof(PermissionEnum.ManageFirmAdminAvailability), Description = "Set own Active/Inactive availability (Firm Admin)" },
            new Permission { PermissionID = (int)PermissionEnum.ViewFirmAdminAvailability, PermissionName = nameof(PermissionEnum.ViewFirmAdminAvailability), Description = "View Firm Admin's availability status" }
        );
    }

    // ROLE-PERMISSIONS - Complete matrix mapping roles to permissions
    private static void SeedRolePermissions(ModelBuilder modelBuilder)
    {
        var rolePermissions = new List<RolePermission>();
        int id = 1;

        void Map(UserRole role, params PermissionEnum[] permissions)
        {
            foreach (var p in permissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RolePermissionID = id++,
                    RoleID = (int)role,
                    PermissionID = (int)p
                });
            }
        }

        Map(UserRole.SuperAdmin,
            PermissionEnum.ManageFirms,
            PermissionEnum.ViewSystemAuditLogs,
            PermissionEnum.ManageDataMigration,
            PermissionEnum.ManageSystemUsers);

        Map(UserRole.FirmAdmin,
            PermissionEnum.ManageFirmUsers,
            PermissionEnum.ViewFirmCaseDirectory,
            PermissionEnum.AssignLawyersToCases,
            PermissionEnum.ManageFirmSettings,
            PermissionEnum.DeleteCases,
            PermissionEnum.ViewFirmCases,
            PermissionEnum.CreateCases,
            PermissionEnum.UpdateCases,
            PermissionEnum.ViewAllDocuments,
            PermissionEnum.DownloadDocuments,
            PermissionEnum.UploadDocuments,
            PermissionEnum.ViewFirmAnalytics,
            // BUG FIX: LoginHistoryController required this permission but it
            // never existed anywhere in the seed data - see PermissionEnum.cs.
            // DeleteLoginHistory is intentionally NOT granted here (SuperAdmin
            // only) to keep this security-relevant audit trail tamper-resistant.
            PermissionEnum.ViewLoginHistory,
            // BUG FIX: same gap as ViewLoginHistory above - AuditLogsController
            // and DashboardController required these but neither was ever
            // seeded, so only SuperAdmin (via the old blanket bypass) could
            // reach them. Both are firm-scoped for FirmAdmin (see
            // GetAuditLogsHandler / GetFirmDashboardHandler).
            PermissionEnum.ViewAuditLogs,
            PermissionEnum.ViewDashboard,
            // Firm/User workflow: FirmAdmin is the only role that manages
            // firm-user membership state and its own availability.
            PermissionEnum.BlockFirmUser,
            PermissionEnum.UnblockFirmUser,
            PermissionEnum.RemoveFirmUser,
            PermissionEnum.ChangeFirmUserRole,
            PermissionEnum.ManageFirmAdminAvailability,
            PermissionEnum.ViewFirmAdminAvailability);

        Map(UserRole.Partner,
            PermissionEnum.ViewFirmCaseDirectory,
            PermissionEnum.AssignLawyersToCases,
            PermissionEnum.AssignCases,
            PermissionEnum.DeleteCases,
            PermissionEnum.ViewFirmCases,
            PermissionEnum.CreateCases,
            PermissionEnum.UpdateCases,
            PermissionEnum.ViewAllDocuments,
            PermissionEnum.DownloadDocuments,
            PermissionEnum.UploadDocuments,
            PermissionEnum.ApproveFilings,
            PermissionEnum.ViewFirmAnalytics,
            PermissionEnum.ViewDashboard,
            PermissionEnum.ViewFirmAdminAvailability);

        Map(UserRole.AssociateLawyer,
            PermissionEnum.ViewAssignedCases,
            PermissionEnum.UploadDocuments,
            PermissionEnum.DownloadAssignedDocuments,
            PermissionEnum.AddCaseNotes,
            PermissionEnum.TrackDeadlines,
            PermissionEnum.LogBillableHours,
            PermissionEnum.ViewDashboard,
            PermissionEnum.ViewFirmAdminAvailability);

        Map(UserRole.Moharrir,
            PermissionEnum.EnterCaseData,
            PermissionEnum.UploadCaseDocuments,
            PermissionEnum.MaintainCaseRecords,
            PermissionEnum.ViewDashboard,
            PermissionEnum.ViewFirmAdminAvailability);

        Map(UserRole.InternParalegal,
            PermissionEnum.ViewDocumentsReadOnly,
            PermissionEnum.DraftDocuments,
            PermissionEnum.PerformResearch,
            PermissionEnum.ViewDashboard,
            PermissionEnum.ViewFirmAdminAvailability);

        modelBuilder.Entity<RolePermission>().HasData(rolePermissions);
    }

    // NOTIFICATION TYPES - Automated notification triggers
    private static void SeedNotificationTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationType>().HasData(
            new NotificationType
            {
                NotificationTypeID = 1,
                TypeName = "DeadlineAlert",
                Description = "Reminder for approaching case deadline",
                IsEmail = true,
                IsSMS = false,
                IsInApp = true,
                IsActive = true
            },
            new NotificationType
            {
                NotificationTypeID = 2,
                TypeName = "HearingReminder",
                Description = "Reminder for upcoming court hearing",
                IsEmail = true,
                IsSMS = false,
                IsInApp = true,
                IsActive = true
            },
            new NotificationType
            {
                NotificationTypeID = 3,
                TypeName = "CaseAssignment",
                Description = "Notification when case is assigned",
                IsEmail = true,
                IsSMS = false,
                IsInApp = true,
                IsActive = true
            },
            new NotificationType
            {
                NotificationTypeID = 4,
                TypeName = "DocumentUploaded",
                Description = "Notification when document uploaded to case",
                IsEmail = false,
                IsSMS = false,
                IsInApp = true,
                IsActive = true
            },
            new NotificationType
            {
                NotificationTypeID = 5,
                TypeName = "CaseStatusChanged",
                Description = "Notification when case status changes",
                IsEmail = true,
                IsSMS = false,
                IsInApp = true,
                IsActive = true
            },
            new NotificationType
            {
                NotificationTypeID = 6,
                TypeName = "FirmAdminRequest",
                Description = "Sent to every Super Admin when someone requests to become a Firm Admin",
                IsEmail = true,
                IsSMS = false,
                IsInApp = true,
                IsActive = true
            },
            new NotificationType
            {
                NotificationTypeID = 7,
                TypeName = "UserJoinRequest",
                Description = "Sent to a firm's Firm Admin(s) when someone requests to join that firm as Partner/Associate/Moharrir/Intern",
                IsEmail = true,
                IsSMS = false,
                IsInApp = true,
                IsActive = true
            },
            new NotificationType
            {
                NotificationTypeID = 8,
                TypeName = "CompleteProfile",
                Description = "Sent to a newly quick-added user (Email + Temp Password only), prompting them to complete their profile",
                IsEmail = true,
                IsSMS = false,
                IsInApp = true,
                IsActive = true
            },
            new NotificationType
            {
                NotificationTypeID = 9,
                TypeName = "FirmMembershipChange",
                Description = "Sent to a firm user when they are blocked, unblocked, or removed from their firm",
                IsEmail = false,
                IsSMS = false,
                IsInApp = true,
                IsActive = true
            }
        );
    }
}