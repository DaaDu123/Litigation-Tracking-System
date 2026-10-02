using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LTSBackend.Migrations
{
    /// <inheritdoc />
    public partial class SeedCaseStatusesAndStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CaseStatus",
                columns: new[] { "StatusID", "ColorCode", "FirmID", "IsActive", "IsClosed", "SequenceNo", "StatusName" },
                values: new object[,]
                {
                    { 1, "#6366F1", null, true, false, 1, "New" },
                    { 2, "#F59E0B", null, true, false, 2, "In Progress" },
                    { 3, "#94A3B8", null, true, false, 3, "On Hold" },
                    { 4, "#10B981", null, true, true, 4, "Disposed" },
                    { 5, "#EF4444", null, true, true, 5, "Closed" }
                });

            migrationBuilder.InsertData(
                table: "CaseStages",
                columns: new[] { "StageID", "Description", "FirmID", "IsActive", "StageName" },
                values: new object[,]
                {
                    { 1, "Case has been filed and is awaiting registration/numbering", null, true, "Filing" },
                    { 2, "Case is being heard; hearings are scheduled or in progress", null, true, "Hearing" },
                    { 3, "Arguments are being presented before the court", null, true, "Arguments" },
                    { 4, "Court has reserved or announced judgment", null, true, "Judgment" },
                    { 5, "Case is under appeal before a higher court", null, true, "Appeal" },
                    { 6, "Judgment/order is being executed or enforced", null, true, "Execution" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CaseStatus",
                keyColumn: "StatusID",
                keyValues: new object[] { 1, 2, 3, 4, 5 });

            migrationBuilder.DeleteData(
                table: "CaseStages",
                keyColumn: "StageID",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6 });
        }
    }
}
