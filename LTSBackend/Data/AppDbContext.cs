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



















public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }



    private ClaimsPrincipal? RequestUser => _httpContextAccessor?.HttpContext?.User;

    private bool IsAuthenticatedRequest => RequestUser?.Identity?.IsAuthenticated == true;

    private bool IsSuperAdminRequest => RequestUser?.FindFirstValue(ClaimTypes.Role) == RoleNames.SuperAdmin;



    private int? RequestFirmId
    {
        get
        {
            var value = RequestUser?.FindFirstValue("FirmID");
            return int.TryParse(value, out var id) ? id : null;
        }
    }









    private bool BypassTenantFilter => !IsAuthenticatedRequest || IsSuperAdminRequest;


    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Firm> Firms { get; set; } = null!;
    public DbSet<FirmAdminRequest> FirmAdminRequests { get; set; } = null!;
    public DbSet<UserJoinRequest> UserJoinRequests { get; set; } = null!;
    public DbSet<UserAccessRequestSlot> UserAccessRequestSlots { get; set; } = null!;
    public DbSet<FirmMembershipEvent> FirmMembershipEvents { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<NotificationType> NotificationTypes { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<UserOtp> UserOtps { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<LoginHistory> LoginHistories { get; set; } = null!;


    public DbSet<AuditLog> AuditLogs { get; set; } = null!;


    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Court> Courts { get; set; } = null!;
    public DbSet<CaseCategory> CaseCategories { get; set; } = null!;
    public DbSet<CaseStatus> CaseStatuses { get; set; } = null!;
    public DbSet<CaseStage> CaseStages { get; set; } = null!;
    public DbSet<DocumentType> DocumentTypes { get; set; } = null!;
    public DbSet<CaseWorkflowTemplate> CaseWorkflowTemplates { get; set; } = null!;
    public DbSet<CaseWorkflowTemplateStage> CaseWorkflowTemplateStages { get; set; } = null!;
    public DbSet<CaseWorkflowTemplateDocument> CaseWorkflowTemplateDocuments { get; set; } = null!;


    public DbSet<Case> Cases { get; set; } = null!;
    public DbSet<CaseParty> CaseParties { get; set; } = null!;
    public DbSet<CaseAssignment> CaseAssignments { get; set; } = null!;
    public DbSet<CaseStatusHistory> CaseStatusHistories { get; set; } = null!;
    public DbSet<CaseMilestone> CaseMilestones { get; set; } = null!;
    public DbSet<CaseWorkflowStage> CaseWorkflowStages { get; set; } = null!;
    public DbSet<CaseDocumentRequirement> CaseDocumentRequirements { get; set; } = null!;


    public DbSet<Hearing> Hearings { get; set; } = null!;
    public DbSet<HearingAttendance> HearingAttendances { get; set; } = null!;
    public DbSet<Deadline> Deadlines { get; set; } = null!;


    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentPermission> DocumentPermissions { get; set; } = null!;
    public DbSet<CaseNote> CaseNotes { get; set; } = null!;


    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserID);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique().HasFilter("[IsDeleted] = 0");





            entity.HasIndex(e => e.Phone).IsUnique().HasDatabaseName("IX_Users_Phone").HasFilter("[Phone] IS NOT NULL AND [IsDeleted] = 0");
            entity.HasIndex(e => e.CNIC).IsUnique().HasDatabaseName("IX_Users_CNIC").HasFilter("[CNIC] IS NOT NULL AND [IsDeleted] = 0");
            entity.HasOne(e => e.Firm).WithMany(f => f.Users).HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.FirmID);
            entity.HasMany(e => e.RefreshTokens).WithOne(r => r.User).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.UserOtps).WithOne(o => o.User).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.PasswordResetTokens).WithOne(t => t.User).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.LoginHistories).WithOne(l => l.User).OnDelete(DeleteBehavior.Cascade);




            entity.HasQueryFilter(e => !e.IsDeleted && (BypassTenantFilter || e.FirmID == RequestFirmId));
        });


        modelBuilder.Entity<Firm>(entity =>
        {
            entity.HasKey(e => e.FirmID);
            entity.Property(e => e.FirmName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.FirmCode).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.FirmCode).IsUnique();
            entity.Property(e => e.MigrationStatus).IsRequired().HasMaxLength(30).HasDefaultValue("None");
            entity.Property(e => e.MigrationNotes).HasMaxLength(500);
        });



        modelBuilder.Entity<FirmAdminRequest>(entity =>
        {
            entity.HasKey(e => e.RequestID);



            entity.Property(e => e.FirmName).HasMaxLength(150);
            entity.Property(e => e.FirmCode).HasMaxLength(30);
            entity.Property(e => e.AdminFullName).HasMaxLength(150);
            entity.Property(e => e.AdminEmail).IsRequired().HasMaxLength(150);



            entity.Property(e => e.AdminPasswordHash).HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            entity.HasIndex(e => e.FirmCode);
            entity.HasIndex(e => e.AdminEmail);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.UserID);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);
        });









        modelBuilder.Entity<UserJoinRequest>(entity =>
        {
            entity.HasKey(e => e.RequestID);



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

        modelBuilder.Entity<UserAccessRequestSlot>(entity =>
        {
            entity.HasKey(e => e.UserID);
            entity.Property(e => e.UserID).ValueGeneratedNever();
            entity.Property(e => e.TargetType).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("CK_UserAccessRequestSlots_TargetType", "[TargetType] IN ('FirmAdmin','SuperAdmin')"));
        });




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


        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleID);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.RoleName).IsUnique();
            entity.HasMany(e => e.RolePermissions).WithOne(rp => rp.Role).OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionID);
            entity.Property(e => e.PermissionName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.PermissionName).IsUnique();
        });


        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(x => x.RolePermissionID);
            entity.HasIndex(x => new { x.RoleID, x.PermissionID }).IsUnique();
            entity.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionID).OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenID);

            entity.Property(e => e.Token).IsRequired().HasMaxLength(64);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => new { e.UserID, e.IsRevoked });

            entity.Property(e => e.ExpiryDate).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade);





            entity.HasQueryFilter(e => BypassTenantFilter || e.User.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<UserOtp>(entity =>
        {
            entity.HasKey(e => e.OtpID);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.OtpCode).IsRequired().HasMaxLength(6);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.UserOtps).HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade).IsRequired(false);
            entity.HasIndex(e => new { e.Email, e.OtpCode });





            entity.HasQueryFilter(e => BypassTenantFilter || e.User == null || e.User.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenID);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.PasswordResetTokens).HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade).IsRequired();


            entity.HasIndex(e => e.TokenHash).IsUnique();




            entity.HasQueryFilter(e => BypassTenantFilter || e.User == null || e.User!.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.LoginID);
            entity.Property(e => e.UserID).IsRequired();
            entity.Property(e => e.LoginTime).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.LoginHistories).HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserID, e.LoginTime }).IsDescending(false, true);






            entity.HasQueryFilter(e => BypassTenantFilter || e.User.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogID);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasIndex(e => e.Timestamp).IsDescending();
            entity.HasIndex(e => e.UserID);


            entity.HasQueryFilter(e => BypassTenantFilter || (e.User != null && e.User.FirmID == RequestFirmId));
        });



        modelBuilder.Entity<Court>(entity =>
        {
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);




            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == null || e.FirmID == RequestFirmId);
        });



        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == null || e.FirmID == RequestFirmId);
        });



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

        modelBuilder.Entity<CaseWorkflowTemplate>(entity =>
        {
            entity.HasOne(e => e.Firm).WithMany().HasForeignKey(e => e.FirmID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryID).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.DefaultDepartment).WithMany().HasForeignKey(e => e.DefaultDepartmentID).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.InitialStatus).WithMany().HasForeignKey(e => e.InitialStatusID).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.Stages).WithOne(s => s.Template).HasForeignKey(s => s.TemplateID).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Documents).WithOne(d => d.Template).HasForeignKey(d => d.TemplateID).OnDelete(DeleteBehavior.Cascade);


            entity.HasIndex(e => new { e.FirmID, e.CategoryID }).IsUnique();

            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == null || e.FirmID == RequestFirmId);
        });

        modelBuilder.Entity<CaseWorkflowTemplateStage>(entity =>
        {
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageID).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(e => new { e.TemplateID, e.StageID }).IsUnique();
            entity.HasIndex(e => new { e.TemplateID, e.SequenceNo }).IsUnique();
            entity.HasQueryFilter(e => BypassTenantFilter || e.Template.FirmID == null || e.Template.FirmID == RequestFirmId);
        });

        modelBuilder.Entity<CaseWorkflowTemplateDocument>(entity =>
        {
            entity.HasOne(e => e.DocumentType).WithMany().HasForeignKey(e => e.DocumentTypeID).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(e => new { e.TemplateID, e.DocumentTypeID }).IsUnique();
            entity.HasQueryFilter(e => BypassTenantFilter || e.Template.FirmID == null || e.Template.FirmID == RequestFirmId);
        });

        modelBuilder.Entity<CaseWorkflowStage>(entity =>
        {
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageID).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(e => new { e.CaseID, e.StageID }).IsUnique();
            entity.HasIndex(e => new { e.CaseID, e.SequenceNo }).IsUnique();
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });

        modelBuilder.Entity<CaseDocumentRequirement>(entity =>
        {
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.DocumentType).WithMany().HasForeignKey(e => e.DocumentTypeID).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(e => new { e.CaseID, e.DocumentTypeID }).IsUnique();
            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });


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


            entity.HasIndex(e => e.StatusID);
            entity.HasIndex(e => e.CourtID);
            entity.HasIndex(e => e.CategoryID);
            entity.HasIndex(e => e.CaseNumber);
            entity.HasIndex(e => e.FirmID);


            entity.HasQueryFilter(e => BypassTenantFilter || e.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<CaseParty>(entity =>
        {
            entity.HasKey(e => e.PartyID);
            entity.Property(e => e.PartyType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PartyName).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Case).WithMany(c => c.CaseParties).HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CaseID);


            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<CaseAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentID);
            entity.Property(e => e.AssignmentType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.LeadCounsel).HasColumnName("IsLeadCounsel");
            entity.HasOne(e => e.Case).WithMany(c => c.CaseAssignments).HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UserID);
            entity.HasIndex(e => new { e.CaseID, e.EndDate });


            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<CaseStatusHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryID);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);


            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<CaseMilestone>(entity =>
        {
            entity.HasKey(e => e.MilestoneID);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CaseID);


            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<Hearing>(entity =>
        {
            entity.HasKey(e => e.HearingID);
            entity.HasOne(e => e.Case).WithMany(c => c.Hearings).HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Court).WithMany().HasForeignKey(e => e.CourtID).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.HearingAttendances).WithOne(ha => ha.Hearing).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.HearingDate);
            entity.HasIndex(e => e.CaseID);


            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<HearingAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceID);
            entity.HasOne(e => e.Hearing).WithMany(h => h.HearingAttendances).HasForeignKey(e => e.HearingID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.HearingID, e.UserID }).IsUnique();


            entity.HasQueryFilter(e => BypassTenantFilter || e.Hearing.Case.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<Deadline>(entity =>
        {
            entity.HasKey(e => e.DeadlineID);
            entity.HasOne(e => e.Case).WithMany(c => c.Deadlines).HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => new { e.CaseID, e.Completed });


            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentID);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.DocumentType).WithMany().HasForeignKey(e => e.DocumentTypeID).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.DocumentPermissions).WithOne(dp => dp.Document).OnDelete(DeleteBehavior.Cascade);





            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<DocumentPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionID);
            entity.HasOne(e => e.Document).WithMany(d => d.DocumentPermissions).HasForeignKey(e => e.DocumentID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany().HasForeignKey(e => e.RoleID).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.DocumentID, e.RoleID });
            entity.HasIndex(e => new { e.DocumentID, e.UserID });


            entity.HasQueryFilter(e => BypassTenantFilter || e.Document.Case.FirmID == RequestFirmId);
        });


        modelBuilder.Entity<CaseNote>(entity =>
        {
            entity.HasKey(e => e.NoteID);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.CaseID);


            entity.HasQueryFilter(e => BypassTenantFilter || e.Case.FirmID == RequestFirmId);
        });
        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.NotificationTypeID);
            entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.TypeName).IsUnique();
        });


        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationID);
            entity.HasOne(e => e.NotificationType).WithMany(t => t.Notifications).HasForeignKey(e => e.NotificationTypeID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Case).WithMany().HasForeignKey(e => e.CaseID).OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.Priority).HasMaxLength(20);




            entity.HasQueryFilter(e => BypassTenantFilter || (e.User != null && e.User.FirmID == RequestFirmId));
        });

        SeedRoles(modelBuilder);
        SeedPermissions(modelBuilder);
        SeedRolePermissions(modelBuilder);
        SeedUsers(modelBuilder);
        SeedNotificationTypes(modelBuilder);
        SeedCaseStatuses(modelBuilder);
        SeedCaseStages(modelBuilder);
    }






    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);








    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(

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


    private static void SeedPermissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasData(

            new Permission { PermissionID = (int)PermissionEnum.ManageFirms, PermissionName = nameof(PermissionEnum.ManageFirms), Description = "Create, block, remove firms" },
            new Permission { PermissionID = (int)PermissionEnum.ViewSystemAuditLogs, PermissionName = nameof(PermissionEnum.ViewSystemAuditLogs), Description = "View system-wide audit logs" },
            new Permission { PermissionID = (int)PermissionEnum.ManageDataMigration, PermissionName = nameof(PermissionEnum.ManageDataMigration), Description = "Manage firm data migration" },
            new Permission { PermissionID = (int)PermissionEnum.ManageSystemUsers, PermissionName = nameof(PermissionEnum.ManageSystemUsers), Description = "Manage all system users" },


            new Permission { PermissionID = (int)PermissionEnum.ManageFirmUsers, PermissionName = nameof(PermissionEnum.ManageFirmUsers), Description = "Create and manage firm users" },
            new Permission { PermissionID = (int)PermissionEnum.ViewFirmCaseDirectory, PermissionName = nameof(PermissionEnum.ViewFirmCaseDirectory), Description = "View all cases in firm" },
            new Permission { PermissionID = (int)PermissionEnum.AssignLawyersToCases, PermissionName = nameof(PermissionEnum.AssignLawyersToCases), Description = "Assign lawyers to cases" },
            new Permission { PermissionID = (int)PermissionEnum.ManageFirmSettings, PermissionName = nameof(PermissionEnum.ManageFirmSettings), Description = "Manage firm settings and billing" },
            new Permission { PermissionID = (int)PermissionEnum.DeleteCases, PermissionName = nameof(PermissionEnum.DeleteCases), Description = "Delete cases" },
            new Permission { PermissionID = (int)PermissionEnum.ViewLoginHistory, PermissionName = nameof(PermissionEnum.ViewLoginHistory), Description = "View this firm's own login history" },
            new Permission { PermissionID = (int)PermissionEnum.DeleteLoginHistory, PermissionName = nameof(PermissionEnum.DeleteLoginHistory), Description = "Delete/cleanup this firm's own login history records" },
            new Permission { PermissionID = (int)PermissionEnum.ViewAuditLogs, PermissionName = nameof(PermissionEnum.ViewAuditLogs), Description = "View this firm's own audit trail" },
            new Permission { PermissionID = (int)PermissionEnum.UploadDocuments, PermissionName = nameof(PermissionEnum.UploadDocuments), Description = "Upload documents" },


            new Permission { PermissionID = (int)PermissionEnum.ViewFirmCases, PermissionName = nameof(PermissionEnum.ViewFirmCases), Description = "View firm cases" },
            new Permission { PermissionID = (int)PermissionEnum.CreateCases, PermissionName = nameof(PermissionEnum.CreateCases), Description = "Create new cases" },
            new Permission { PermissionID = (int)PermissionEnum.UpdateCases, PermissionName = nameof(PermissionEnum.UpdateCases), Description = "Update case information" },
            new Permission { PermissionID = (int)PermissionEnum.AssignCases, PermissionName = nameof(PermissionEnum.AssignCases), Description = "Assign cases to lawyers" },
            new Permission { PermissionID = (int)PermissionEnum.ViewAllDocuments, PermissionName = nameof(PermissionEnum.ViewAllDocuments), Description = "View all case documents" },
            new Permission { PermissionID = (int)PermissionEnum.DownloadDocuments, PermissionName = nameof(PermissionEnum.DownloadDocuments), Description = "Download documents" },
            new Permission { PermissionID = (int)PermissionEnum.ApproveFilings, PermissionName = nameof(PermissionEnum.ApproveFilings), Description = "Approve critical filings" },
            new Permission { PermissionID = (int)PermissionEnum.ViewFirmAnalytics, PermissionName = nameof(PermissionEnum.ViewFirmAnalytics), Description = "View firm analytics and reports" },


            new Permission { PermissionID = (int)PermissionEnum.ViewAssignedCases, PermissionName = nameof(PermissionEnum.ViewAssignedCases), Description = "View assigned cases only" },
            new Permission { PermissionID = (int)PermissionEnum.DownloadAssignedDocuments, PermissionName = nameof(PermissionEnum.DownloadAssignedDocuments), Description = "Download assigned case documents" },
            new Permission { PermissionID = (int)PermissionEnum.AddCaseNotes, PermissionName = nameof(PermissionEnum.AddCaseNotes), Description = "Add notes to cases" },
            new Permission { PermissionID = (int)PermissionEnum.TrackDeadlines, PermissionName = nameof(PermissionEnum.TrackDeadlines), Description = "Track case deadlines" },
            new Permission { PermissionID = (int)PermissionEnum.LogBillableHours, PermissionName = nameof(PermissionEnum.LogBillableHours), Description = "Log billable hours" },


            new Permission { PermissionID = (int)PermissionEnum.EnterCaseData, PermissionName = nameof(PermissionEnum.EnterCaseData), Description = "Enter case data" },
            new Permission { PermissionID = (int)PermissionEnum.UploadCaseDocuments, PermissionName = nameof(PermissionEnum.UploadCaseDocuments), Description = "Upload case documents" },
            new Permission { PermissionID = (int)PermissionEnum.ViewDocumentsIfPermitted, PermissionName = nameof(PermissionEnum.ViewDocumentsIfPermitted), Description = "View documents if permitted" },
            new Permission { PermissionID = (int)PermissionEnum.DownloadDocumentsIfPermitted, PermissionName = nameof(PermissionEnum.DownloadDocumentsIfPermitted), Description = "Download documents if permitted" },
            new Permission { PermissionID = (int)PermissionEnum.MaintainCaseRecords, PermissionName = nameof(PermissionEnum.MaintainCaseRecords), Description = "Maintain case records" },


            new Permission { PermissionID = (int)PermissionEnum.ViewDocumentsReadOnly, PermissionName = nameof(PermissionEnum.ViewDocumentsReadOnly), Description = "View documents (read-only)" },
            new Permission { PermissionID = (int)PermissionEnum.DraftDocuments, PermissionName = nameof(PermissionEnum.DraftDocuments), Description = "Draft legal documents" },
            new Permission { PermissionID = (int)PermissionEnum.PerformResearch, PermissionName = nameof(PermissionEnum.PerformResearch), Description = "Perform legal research" },


            new Permission { PermissionID = (int)PermissionEnum.ViewDashboard, PermissionName = nameof(PermissionEnum.ViewDashboard), Description = "View one's own role-scoped dashboard" },


            new Permission { PermissionID = (int)PermissionEnum.BlockFirmUser, PermissionName = nameof(PermissionEnum.BlockFirmUser), Description = "Block a firm user" },
            new Permission { PermissionID = (int)PermissionEnum.UnblockFirmUser, PermissionName = nameof(PermissionEnum.UnblockFirmUser), Description = "Unblock a previously blocked firm user" },
            new Permission { PermissionID = (int)PermissionEnum.RemoveFirmUser, PermissionName = nameof(PermissionEnum.RemoveFirmUser), Description = "Remove a user from the firm" },
            new Permission { PermissionID = (int)PermissionEnum.ChangeFirmUserRole, PermissionName = nameof(PermissionEnum.ChangeFirmUserRole), Description = "Change a firm user's role" },
            new Permission { PermissionID = (int)PermissionEnum.ManageFirmAdminAvailability, PermissionName = nameof(PermissionEnum.ManageFirmAdminAvailability), Description = "Set own Active/Inactive availability (Firm Admin)" },
            new Permission { PermissionID = (int)PermissionEnum.ViewFirmAdminAvailability, PermissionName = nameof(PermissionEnum.ViewFirmAdminAvailability), Description = "View Firm Admin's availability status" }
        );
    }


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




            PermissionEnum.ViewLoginHistory,





            PermissionEnum.ViewAuditLogs,
            PermissionEnum.ViewDashboard,


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








    private static void SeedCaseStatuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaseStatus>().HasData(
            new CaseStatus { StatusID = 1, FirmID = null, StatusName = "New", SequenceNo = 1, ColorCode = "#6366F1", IsClosed = false, IsActive = true },
            new CaseStatus { StatusID = 2, FirmID = null, StatusName = "In Progress", SequenceNo = 2, ColorCode = "#F59E0B", IsClosed = false, IsActive = true },
            new CaseStatus { StatusID = 3, FirmID = null, StatusName = "On Hold", SequenceNo = 3, ColorCode = "#94A3B8", IsClosed = false, IsActive = true },
            new CaseStatus { StatusID = 4, FirmID = null, StatusName = "Disposed", SequenceNo = 4, ColorCode = "#10B981", IsClosed = true, IsActive = true },
            new CaseStatus { StatusID = 5, FirmID = null, StatusName = "Closed", SequenceNo = 5, ColorCode = "#EF4444", IsClosed = true, IsActive = true }
        );
    }




    private static void SeedCaseStages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaseStage>().HasData(
            new CaseStage { StageID = 1, FirmID = null, StageName = "Filing", Description = "Case has been filed and is awaiting registration/numbering", IsActive = true },
            new CaseStage { StageID = 2, FirmID = null, StageName = "Hearing", Description = "Case is being heard; hearings are scheduled or in progress", IsActive = true },
            new CaseStage { StageID = 3, FirmID = null, StageName = "Arguments", Description = "Arguments are being presented before the court", IsActive = true },
            new CaseStage { StageID = 4, FirmID = null, StageName = "Judgment", Description = "Court has reserved or announced judgment", IsActive = true },
            new CaseStage { StageID = 5, FirmID = null, StageName = "Appeal", Description = "Case is under appeal before a higher court", IsActive = true },
            new CaseStage { StageID = 6, FirmID = null, StageName = "Execution", Description = "Judgment/order is being executed or enforced", IsActive = true }
        );
    }
}