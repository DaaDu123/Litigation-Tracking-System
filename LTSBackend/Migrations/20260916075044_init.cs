using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LTSBackend.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FirmAdminRequests",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FirmCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AdminFullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AdminEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AdminPasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AdminPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedFirmID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmAdminRequests", x => x.RequestID);
                });

            migrationBuilder.CreateTable(
                name: "Firms",
                columns: table => new
                {
                    FirmID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FirmCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CustomDomain = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MigrationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "None"),
                    MigrationRequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MigrationRequestedBy = table.Column<int>(type: "int", nullable: true),
                    MigrationCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MigrationNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    BlockedReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BlockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firms", x => x.FirmID);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTypes",
                columns: table => new
                {
                    NotificationTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEmail = table.Column<bool>(type: "bit", nullable: false),
                    IsSMS = table.Column<bool>(type: "bit", nullable: false),
                    IsInApp = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTypes", x => x.NotificationTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "CaseCategories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: true),
                    CategoryName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseCategories", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_CaseCategories_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseStages",
                columns: table => new
                {
                    StageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: true),
                    StageName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseStages", x => x.StageID);
                    table.ForeignKey(
                        name: "FK_CaseStages_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseStatus",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: true),
                    StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseStatus", x => x.StatusID);
                    table.ForeignKey(
                        name: "FK_CaseStatus_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Courts",
                columns: table => new
                {
                    CourtID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: true),
                    CourtName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CourtType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Jurisdiction = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courts", x => x.CourtID);
                    table.ForeignKey(
                        name: "FK_Courts_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DepartmentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentID);
                    table.ForeignKey(
                        name: "FK_Departments_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: true),
                    TypeName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.DocumentTypeID);
                    table.ForeignKey(
                        name: "FK_DocumentTypes_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserJoinRequests",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestedRoleID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserJoinRequests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_UserJoinRequests_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RolePermissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    PermissionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.RolePermissionID);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionID",
                        column: x => x.PermissionID,
                        principalTable: "Permissions",
                        principalColumn: "PermissionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProfileImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    FirmID = table.Column<int>(type: "int", nullable: true),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsReleasedForReuse = table.Column<bool>(type: "bit", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordChangedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordID = table.Column<int>(type: "int", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    CaseID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: false),
                    InternalReferenceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CaseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CaseTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CaseDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CourtID = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    StatusID = table.Column<int>(type: "int", nullable: false),
                    StageID = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubjectMatter = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilingDate = table.Column<DateTime>(type: "date", nullable: false),
                    InstitutionDate = table.Column<DateTime>(type: "date", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpectedDisposalDate = table.Column<DateTime>(type: "date", nullable: true),
                    NextHearingDate = table.Column<DateTime>(type: "date", nullable: true),
                    UpcomingDeadline = table.Column<DateTime>(type: "date", nullable: true),
                    ClaimedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PotentialLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialImplication = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ResponsibleDepartmentID = table.Column<int>(type: "int", nullable: true),
                    CurrentLegalOfficerID = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.CaseID);
                    table.ForeignKey(
                        name: "FK_Cases_CaseCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "CaseCategories",
                        principalColumn: "CategoryID");
                    table.ForeignKey(
                        name: "FK_Cases_CaseStages_StageID",
                        column: x => x.StageID,
                        principalTable: "CaseStages",
                        principalColumn: "StageID");
                    table.ForeignKey(
                        name: "FK_Cases_CaseStatus_StatusID",
                        column: x => x.StatusID,
                        principalTable: "CaseStatus",
                        principalColumn: "StatusID");
                    table.ForeignKey(
                        name: "FK_Cases_Courts_CourtID",
                        column: x => x.CourtID,
                        principalTable: "Courts",
                        principalColumn: "CourtID");
                    table.ForeignKey(
                        name: "FK_Cases_Departments_ResponsibleDepartmentID",
                        column: x => x.ResponsibleDepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID");
                    table.ForeignKey(
                        name: "FK_Cases_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cases_Users_CurrentLegalOfficerID",
                        column: x => x.CurrentLegalOfficerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoginHistories",
                columns: table => new
                {
                    LoginID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogoutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsLoggedOut = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginHistories", x => x.LoginID);
                    table.ForeignKey(
                        name: "FK_LoginHistories_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    TokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.TokenID);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshTokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenID);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOtps",
                columns: table => new
                {
                    OtpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOtps", x => x.OtpID);
                    table.ForeignKey(
                        name: "FK_UserOtps_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseAssignments",
                columns: table => new
                {
                    AssignmentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    AssignmentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsLeadCounsel = table.Column<bool>(type: "bit", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseAssignments", x => x.AssignmentID);
                    table.ForeignKey(
                        name: "FK_CaseAssignments_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseAssignments_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseMilestones",
                columns: table => new
                {
                    MilestoneID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    Milestone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MilestoneDate = table.Column<DateTime>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CompletedBy = table.Column<int>(type: "int", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseMilestones", x => x.MilestoneID);
                    table.ForeignKey(
                        name: "FK_CaseMilestones_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseNotes",
                columns: table => new
                {
                    NoteID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseNotes", x => x.NoteID);
                    table.ForeignKey(
                        name: "FK_CaseNotes_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseNotes_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseParties",
                columns: table => new
                {
                    PartyID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    PartyType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PartyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Organization = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CNIC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NTN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LawyerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseParties", x => x.PartyID);
                    table.ForeignKey(
                        name: "FK_CaseParties_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseStatusHistory",
                columns: table => new
                {
                    HistoryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    OldStatusID = table.Column<int>(type: "int", nullable: true),
                    NewStatusID = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<int>(type: "int", nullable: false),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseStatusHistory", x => x.HistoryID);
                    table.ForeignKey(
                        name: "FK_CaseStatusHistory_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deadlines",
                columns: table => new
                {
                    DeadlineID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    DeadlineType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    ReminderDays = table.Column<int>(type: "int", nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deadlines", x => x.DeadlineID);
                    table.ForeignKey(
                        name: "FK_Deadlines_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedBy = table.Column<int>(type: "int", nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentID);
                    table.ForeignKey(
                        name: "FK_Documents_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentTypes_DocumentTypeID",
                        column: x => x.DocumentTypeID,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeID");
                });

            migrationBuilder.CreateTable(
                name: "Hearings",
                columns: table => new
                {
                    HearingID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    CourtID = table.Column<int>(type: "int", nullable: false),
                    HearingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourtRoom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JudgeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NextHearingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hearings", x => x.HearingID);
                    table.ForeignKey(
                        name: "FK_Hearings_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hearings_Courts_CourtID",
                        column: x => x.CourtID,
                        principalTable: "Courts",
                        principalColumn: "CourtID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTypeID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CaseID = table.Column<long>(type: "bigint", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID");
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationTypes_NotificationTypeID",
                        column: x => x.NotificationTypeID,
                        principalTable: "NotificationTypes",
                        principalColumn: "NotificationTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentPermissions",
                columns: table => new
                {
                    PermissionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentID = table.Column<long>(type: "bigint", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanDownload = table.Column<bool>(type: "bit", nullable: false),
                    CanUpload = table.Column<bool>(type: "bit", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentPermissions", x => x.PermissionID);
                    table.ForeignKey(
                        name: "FK_DocumentPermissions_Documents_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "Documents",
                        principalColumn: "DocumentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentPermissions_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentPermissions_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HearingAttendance",
                columns: table => new
                {
                    AttendanceID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HearingID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    AttendanceRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Present = table.Column<bool>(type: "bit", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HearingAttendance", x => x.AttendanceID);
                    table.ForeignKey(
                        name: "FK_HearingAttendance_Hearings_HearingID",
                        column: x => x.HearingID,
                        principalTable: "Hearings",
                        principalColumn: "HearingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HearingAttendance_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "NotificationTypeID", "Description", "IsActive", "IsEmail", "IsInApp", "IsSMS", "TypeName" },
                values: new object[,]
                {
                    { 1, "Reminder for approaching case deadline", true, true, true, false, "DeadlineAlert" },
                    { 2, "Reminder for upcoming court hearing", true, true, true, false, "HearingReminder" },
                    { 3, "Notification when case is assigned", true, true, true, false, "CaseAssignment" },
                    { 4, "Notification when document uploaded to case", true, false, true, false, "DocumentUploaded" },
                    { 5, "Notification when case status changes", true, true, true, false, "CaseStatusChanged" },
                    { 6, "Sent to every Super Admin when someone requests to become a Firm Admin", true, true, true, false, "FirmAdminRequest" },
                    { 7, "Sent to a firm's Firm Admin(s) when someone requests to join that firm as Partner/Associate/Moharrir/Intern", true, true, true, false, "UserJoinRequest" },
                    { 8, "Sent to a newly quick-added user (Email + Temp Password only), prompting them to complete their profile", true, true, true, false, "CompleteProfile" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionID", "Description", "PermissionName" },
                values: new object[,]
                {
                    { 101, "Create, block, remove firms", "ManageFirms" },
                    { 102, "View system-wide audit logs", "ViewSystemAuditLogs" },
                    { 103, "Manage firm data migration", "ManageDataMigration" },
                    { 104, "Manage all system users", "ManageSystemUsers" },
                    { 201, "Create and manage firm users", "ManageFirmUsers" },
                    { 202, "View all cases in firm", "ViewFirmCaseDirectory" },
                    { 203, "Assign lawyers to cases", "AssignLawyersToCases" },
                    { 204, "Manage firm settings and billing", "ManageFirmSettings" },
                    { 205, "Delete cases", "DeleteCases" },
                    { 206, "View this firm's own login history", "ViewLoginHistory" },
                    { 207, "Delete/cleanup this firm's own login history records", "DeleteLoginHistory" },
                    { 208, "View this firm's own audit trail", "ViewAuditLogs" },
                    { 301, "View firm cases", "ViewFirmCases" },
                    { 302, "Create new cases", "CreateCases" },
                    { 303, "Update case information", "UpdateCases" },
                    { 304, "Assign cases to lawyers", "AssignCases" },
                    { 305, "View all case documents", "ViewAllDocuments" },
                    { 306, "Download documents", "DownloadDocuments" },
                    { 307, "Approve critical filings", "ApproveFilings" },
                    { 308, "View firm analytics and reports", "ViewFirmAnalytics" },
                    { 401, "View assigned cases only", "ViewAssignedCases" },
                    { 402, "Upload documents", "UploadDocuments" },
                    { 403, "Download assigned case documents", "DownloadAssignedDocuments" },
                    { 404, "Add notes to cases", "AddCaseNotes" },
                    { 405, "Track case deadlines", "TrackDeadlines" },
                    { 406, "Log billable hours", "LogBillableHours" },
                    { 501, "Enter case data", "EnterCaseData" },
                    { 502, "Upload case documents", "UploadCaseDocuments" },
                    { 503, "View documents if permitted", "ViewDocumentsIfPermitted" },
                    { 504, "Download documents if permitted", "DownloadDocumentsIfPermitted" },
                    { 505, "Maintain case records", "MaintainCaseRecords" },
                    { 601, "View documents (read-only)", "ViewDocumentsReadOnly" },
                    { 602, "Draft legal documents", "DraftDocuments" },
                    { 603, "Perform legal research", "PerformResearch" },
                    { 701, "View one's own role-scoped dashboard", "ViewDashboard" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleID", "Description", "IsActive", "IsSystemRole", "RoleName" },
                values: new object[,]
                {
                    { 1, "System-wide management and data custody", true, true, "SuperAdmin" },
                    { 2, "Workspace owner - manages specific law firm", true, false, "FirmAdmin" },
                    { 3, "Senior lawyer - supervises case teams", true, false, "Partner" },
                    { 4, "Day-to-day legal work", true, false, "AssociateLawyer" },
                    { 5, "Legal clerk / Data entry operator", true, false, "Moharrir" },
                    { 6, "Temporary staff / Junior assistant", true, false, "InternParalegal" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RolePermissionID", "PermissionID", "RoleID" },
                values: new object[,]
                {
                    { 1, 101, 1 },
                    { 2, 102, 1 },
                    { 3, 103, 1 },
                    { 4, 104, 1 },
                    { 5, 201, 2 },
                    { 6, 202, 2 },
                    { 7, 203, 2 },
                    { 8, 204, 2 },
                    { 9, 205, 2 },
                    { 10, 301, 2 },
                    { 11, 302, 2 },
                    { 12, 303, 2 },
                    { 13, 305, 2 },
                    { 14, 306, 2 },
                    { 15, 402, 2 },
                    { 16, 308, 2 },
                    { 17, 206, 2 },
                    { 18, 208, 2 },
                    { 19, 701, 2 },
                    { 20, 202, 3 },
                    { 21, 203, 3 },
                    { 22, 304, 3 },
                    { 23, 205, 3 },
                    { 24, 301, 3 },
                    { 25, 302, 3 },
                    { 26, 303, 3 },
                    { 27, 305, 3 },
                    { 28, 306, 3 },
                    { 29, 402, 3 },
                    { 30, 307, 3 },
                    { 31, 308, 3 },
                    { 32, 701, 3 },
                    { 33, 401, 4 },
                    { 34, 402, 4 },
                    { 35, 403, 4 },
                    { 36, 404, 4 },
                    { 37, 405, 4 },
                    { 38, 406, 4 },
                    { 39, 701, 4 },
                    { 40, 501, 5 },
                    { 41, 502, 5 },
                    { 42, 505, 5 },
                    { 43, 701, 5 },
                    { 44, 601, 6 },
                    { 45, 602, 6 },
                    { 46, 603, 6 },
                    { 47, 701, 6 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Department", "Designation", "Email", "EmployeeNo", "FailedLoginAttempts", "FirmID", "FullName", "IsActive", "IsDeleted", "IsExternal", "IsReleasedForReuse", "LastLogin", "LockoutEndUtc", "PasswordChangedDate", "PasswordHash", "Phone", "ProfileImage", "RoleID", "SecurityStamp", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System Administrator", "saadmuhammad19115@gmail.com", "", 0, null, "Super Administrator", true, false, false, false, null, null, null, "$2b$12$AGEF6nAJGVAKB/AtUDhyFuq23a7GuZLpG4g7dUeYvYPXmEimSSXN6", null, null, 1, "SEED-STAMP-USER-0001", null });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserID",
                table: "AuditLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseAssignments_CaseID_EndDate",
                table: "CaseAssignments",
                columns: new[] { "CaseID", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CaseAssignments_UserID",
                table: "CaseAssignments",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseCategories_FirmID",
                table: "CaseCategories",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseMilestones_CaseID",
                table: "CaseMilestones",
                column: "CaseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotes_CaseID",
                table: "CaseNotes",
                column: "CaseID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotes_UserID",
                table: "CaseNotes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseParties_CaseID",
                table: "CaseParties",
                column: "CaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CaseNumber",
                table: "Cases",
                column: "CaseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CategoryID",
                table: "Cases",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CourtID",
                table: "Cases",
                column: "CourtID");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CurrentLegalOfficerID",
                table: "Cases",
                column: "CurrentLegalOfficerID");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_FirmID",
                table: "Cases",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_InternalReferenceNo",
                table: "Cases",
                column: "InternalReferenceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ResponsibleDepartmentID",
                table: "Cases",
                column: "ResponsibleDepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_StageID",
                table: "Cases",
                column: "StageID");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_StatusID",
                table: "Cases",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStages_FirmID",
                table: "CaseStages",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStatus_FirmID",
                table: "CaseStatus",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStatusHistory_CaseID",
                table: "CaseStatusHistory",
                column: "CaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_FirmID",
                table: "Courts",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_Deadlines_CaseID_Completed",
                table: "Deadlines",
                columns: new[] { "CaseID", "Completed" });

            migrationBuilder.CreateIndex(
                name: "IX_Deadlines_DueDate",
                table: "Deadlines",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_FirmID",
                table: "Departments",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPermissions_DocumentID_RoleID",
                table: "DocumentPermissions",
                columns: new[] { "DocumentID", "RoleID" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPermissions_DocumentID_UserID",
                table: "DocumentPermissions",
                columns: new[] { "DocumentID", "UserID" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPermissions_RoleID",
                table: "DocumentPermissions",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPermissions_UserID",
                table: "DocumentPermissions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CaseID",
                table: "Documents",
                column: "CaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentTypeID",
                table: "Documents",
                column: "DocumentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_FirmID",
                table: "DocumentTypes",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_FirmAdminRequests_AdminEmail",
                table: "FirmAdminRequests",
                column: "AdminEmail");

            migrationBuilder.CreateIndex(
                name: "IX_FirmAdminRequests_FirmCode",
                table: "FirmAdminRequests",
                column: "FirmCode");

            migrationBuilder.CreateIndex(
                name: "IX_FirmAdminRequests_Status",
                table: "FirmAdminRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Firms_FirmCode",
                table: "Firms",
                column: "FirmCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HearingAttendance_HearingID_UserID",
                table: "HearingAttendance",
                columns: new[] { "HearingID", "UserID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HearingAttendance_UserID",
                table: "HearingAttendance",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Hearings_CaseID",
                table: "Hearings",
                column: "CaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Hearings_CourtID",
                table: "Hearings",
                column: "CourtID");

            migrationBuilder.CreateIndex(
                name: "IX_Hearings_HearingDate",
                table: "Hearings",
                column: "HearingDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_UserID_LoginTime",
                table: "LoginHistories",
                columns: new[] { "UserID", "LoginTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CaseID",
                table: "Notifications",
                column: "CaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationTypeID",
                table: "Notifications",
                column: "NotificationTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserID",
                table: "Notifications",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTypes_TypeName",
                table: "NotificationTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_TokenHash",
                table: "PasswordResetTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserID",
                table: "PasswordResetTokens",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionName",
                table: "Permissions",
                column: "PermissionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserID_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "UserID", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionID",
                table: "RolePermissions",
                column: "PermissionID");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleID_PermissionID",
                table: "RolePermissions",
                columns: new[] { "RoleID", "PermissionID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserJoinRequests_Email",
                table: "UserJoinRequests",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_UserJoinRequests_FirmID",
                table: "UserJoinRequests",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_UserJoinRequests_Status",
                table: "UserJoinRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserOtps_Email_OtpCode",
                table: "UserOtps",
                columns: new[] { "Email", "OtpCode" });

            migrationBuilder.CreateIndex(
                name: "IX_UserOtps_UserID",
                table: "UserOtps",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_FirmID",
                table: "Users",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CaseAssignments");

            migrationBuilder.DropTable(
                name: "CaseMilestones");

            migrationBuilder.DropTable(
                name: "CaseNotes");

            migrationBuilder.DropTable(
                name: "CaseParties");

            migrationBuilder.DropTable(
                name: "CaseStatusHistory");

            migrationBuilder.DropTable(
                name: "Deadlines");

            migrationBuilder.DropTable(
                name: "DocumentPermissions");

            migrationBuilder.DropTable(
                name: "FirmAdminRequests");

            migrationBuilder.DropTable(
                name: "HearingAttendance");

            migrationBuilder.DropTable(
                name: "LoginHistories");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserJoinRequests");

            migrationBuilder.DropTable(
                name: "UserOtps");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Hearings");

            migrationBuilder.DropTable(
                name: "NotificationTypes");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "CaseCategories");

            migrationBuilder.DropTable(
                name: "CaseStages");

            migrationBuilder.DropTable(
                name: "CaseStatus");

            migrationBuilder.DropTable(
                name: "Courts");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Firms");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
