using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LTSBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmUserWorkflowV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CNIC",
                table: "Users",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InactiveFromUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InactiveReason",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InactiveUntilUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileCompleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastFirmID",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MembershipBlockedAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MembershipBlockedByUserID",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MembershipBlockedReason",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MembershipRemovedAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MembershipRemovedByUserID",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MembershipRemovedReason",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MembershipStatus",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "UserJoinRequests",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "UserJoinRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserJoinRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "UserJoinRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirmName",
                table: "FirmAdminRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "FirmCode",
                table: "FirmAdminRequests",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "AdminFullName",
                table: "FirmAdminRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateTable(
                name: "FirmMembershipEvents",
                columns: table => new
                {
                    EventID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PerformedByUserID = table.Column<int>(type: "int", nullable: true),
                    PerformedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmMembershipEvents", x => x.EventID);
                    table.ForeignKey(
                        name: "FK_FirmMembershipEvents_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FirmMembershipEvents_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "NotificationTypeID", "Description", "IsActive", "IsEmail", "IsInApp", "IsSMS", "TypeName" },
                values: new object[] { 9, "Sent to a firm user when they are blocked, unblocked, or removed from their firm", true, false, true, false, "FirmMembershipChange" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionID", "Description", "PermissionName" },
                values: new object[,]
                {
                    { 209, "Block a firm user", "BlockFirmUser" },
                    { 210, "Unblock a previously blocked firm user", "UnblockFirmUser" },
                    { 211, "Remove a user from the firm", "RemoveFirmUser" },
                    { 212, "Change a firm user's role", "ChangeFirmUserRole" },
                    { 213, "Set own Active/Inactive availability (Firm Admin)", "ManageFirmAdminAvailability" },
                    { 214, "View Firm Admin's availability status", "ViewFirmAdminAvailability" }
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 20,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 209, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 21,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 210, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 22,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 211, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 23,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 212, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 24,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 213, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 25,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 214, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 26,
                column: "PermissionID",
                value: 202);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 27,
                column: "PermissionID",
                value: 203);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 28,
                column: "PermissionID",
                value: 304);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 29,
                column: "PermissionID",
                value: 205);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 30,
                column: "PermissionID",
                value: 301);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 31,
                column: "PermissionID",
                value: 302);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 32,
                column: "PermissionID",
                value: 303);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 33,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 305, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 34,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 306, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 35,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 402, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 36,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 307, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 37,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 308, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 38,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 701, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 39,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 214, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 40,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 401, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 41,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 402, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 42,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 403, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 43,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 404, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 44,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 405, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 45,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 406, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 46,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 701, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 47,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 214, 4 });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RolePermissionID", "PermissionID", "RoleID" },
                values: new object[,]
                {
                    { 48, 501, 5 },
                    { 49, 502, 5 },
                    { 50, 505, 5 },
                    { 51, 701, 5 },
                    { 53, 601, 6 },
                    { 54, 602, 6 },
                    { 55, 603, 6 },
                    { 56, 701, 6 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CNIC", "InactiveFromUtc", "InactiveReason", "InactiveUntilUtc", "IsAvailable", "IsProfileCompleted", "LastFirmID", "MembershipBlockedAtUtc", "MembershipBlockedByUserID", "MembershipBlockedReason", "MembershipRemovedAtUtc", "MembershipRemovedByUserID", "MembershipRemovedReason", "MembershipStatus" },
                values: new object[] { null, null, null, null, true, true, null, null, null, null, null, null, null, "Active" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RolePermissionID", "PermissionID", "RoleID" },
                values: new object[,]
                {
                    { 52, 214, 5 },
                    { 57, 214, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CNIC",
                table: "Users",
                column: "CNIC",
                unique: true,
                filter: "[CNIC] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Phone",
                table: "Users",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserJoinRequests_UserID",
                table: "UserJoinRequests",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FirmMembershipEvents_FirmID",
                table: "FirmMembershipEvents",
                column: "FirmID");

            migrationBuilder.CreateIndex(
                name: "IX_FirmMembershipEvents_UserID_FirmID",
                table: "FirmMembershipEvents",
                columns: new[] { "UserID", "FirmID" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserJoinRequests_Users_UserID",
                table: "UserJoinRequests",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserJoinRequests_Users_UserID",
                table: "UserJoinRequests");

            migrationBuilder.DropTable(
                name: "FirmMembershipEvents");

            migrationBuilder.DropIndex(
                name: "IX_Users_CNIC",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Phone",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserJoinRequests_UserID",
                table: "UserJoinRequests");

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "NotificationTypeID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionID",
                keyValue: 209);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionID",
                keyValue: 210);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionID",
                keyValue: 211);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionID",
                keyValue: 212);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionID",
                keyValue: 213);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionID",
                keyValue: 214);

            migrationBuilder.DropColumn(
                name: "CNIC",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InactiveFromUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InactiveReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InactiveUntilUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsProfileCompleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastFirmID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipBlockedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipBlockedByUserID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipBlockedReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipRemovedAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipRemovedByUserID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipRemovedReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "UserJoinRequests");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "UserJoinRequests",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "UserJoinRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserJoinRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirmName",
                table: "FirmAdminRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirmCode",
                table: "FirmAdminRequests",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdminFullName",
                table: "FirmAdminRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 20,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 202, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 21,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 203, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 22,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 304, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 23,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 205, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 24,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 301, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 25,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 302, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 26,
                column: "PermissionID",
                value: 303);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 27,
                column: "PermissionID",
                value: 305);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 28,
                column: "PermissionID",
                value: 306);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 29,
                column: "PermissionID",
                value: 402);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 30,
                column: "PermissionID",
                value: 307);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 31,
                column: "PermissionID",
                value: 308);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 32,
                column: "PermissionID",
                value: 701);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 33,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 401, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 34,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 402, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 35,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 403, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 36,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 404, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 37,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 405, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 38,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 406, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 39,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 701, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 40,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 501, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 41,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 502, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 42,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 505, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 43,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 701, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 44,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 601, 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 45,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 602, 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 46,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 603, 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionID",
                keyValue: 47,
                columns: new[] { "PermissionID", "RoleID" },
                values: new object[] { 701, 6 });
        }
    }
}
