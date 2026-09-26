using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LTSBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmAdminRequestFromAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AdminPasswordHash",
                table: "FirmAdminRequests",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "FirmAdminRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirmAdminRequests_UserID",
                table: "FirmAdminRequests",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_FirmAdminRequests_Users_UserID",
                table: "FirmAdminRequests",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FirmAdminRequests_Users_UserID",
                table: "FirmAdminRequests");

            migrationBuilder.DropIndex(
                name: "IX_FirmAdminRequests_UserID",
                table: "FirmAdminRequests");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "FirmAdminRequests");

            migrationBuilder.AlterColumn<string>(
                name: "AdminPasswordHash",
                table: "FirmAdminRequests",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
