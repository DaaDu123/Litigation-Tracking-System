using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LTSBackend.Migrations
{
    /// <inheritdoc />
    public partial class updatemasterdataworkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchiveReason",
                table: "Cases",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArchivedBy",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedDate",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowTemplateID",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowTemplateVersion",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CaseDocumentRequirements",
                columns: table => new
                {
                    RequirementID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseDocumentRequirements", x => x.RequirementID);
                    table.ForeignKey(
                        name: "FK_CaseDocumentRequirements_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseDocumentRequirements_DocumentTypes_DocumentTypeID",
                        column: x => x.DocumentTypeID,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeID");
                });

            migrationBuilder.CreateTable(
                name: "CaseWorkflowStages",
                columns: table => new
                {
                    CaseWorkflowStageID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<long>(type: "bigint", nullable: false),
                    StageID = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseWorkflowStages", x => x.CaseWorkflowStageID);
                    table.ForeignKey(
                        name: "FK_CaseWorkflowStages_CaseStages_StageID",
                        column: x => x.StageID,
                        principalTable: "CaseStages",
                        principalColumn: "StageID");
                    table.ForeignKey(
                        name: "FK_CaseWorkflowStages_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseWorkflowTemplates",
                columns: table => new
                {
                    TemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmID = table.Column<int>(type: "int", nullable: true),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    DefaultDepartmentID = table.Column<int>(type: "int", nullable: true),
                    InitialStatusID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseWorkflowTemplates", x => x.TemplateID);
                    table.ForeignKey(
                        name: "FK_CaseWorkflowTemplates_CaseCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "CaseCategories",
                        principalColumn: "CategoryID");
                    table.ForeignKey(
                        name: "FK_CaseWorkflowTemplates_CaseStatus_InitialStatusID",
                        column: x => x.InitialStatusID,
                        principalTable: "CaseStatus",
                        principalColumn: "StatusID");
                    table.ForeignKey(
                        name: "FK_CaseWorkflowTemplates_Departments_DefaultDepartmentID",
                        column: x => x.DefaultDepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID");
                    table.ForeignKey(
                        name: "FK_CaseWorkflowTemplates_Firms_FirmID",
                        column: x => x.FirmID,
                        principalTable: "Firms",
                        principalColumn: "FirmID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseWorkflowTemplateDocuments",
                columns: table => new
                {
                    TemplateDocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateID = table.Column<int>(type: "int", nullable: false),
                    DocumentTypeID = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseWorkflowTemplateDocuments", x => x.TemplateDocumentID);
                    table.ForeignKey(
                        name: "FK_CaseWorkflowTemplateDocuments_CaseWorkflowTemplates_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "CaseWorkflowTemplates",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseWorkflowTemplateDocuments_DocumentTypes_DocumentTypeID",
                        column: x => x.DocumentTypeID,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeID");
                });

            migrationBuilder.CreateTable(
                name: "CaseWorkflowTemplateStages",
                columns: table => new
                {
                    TemplateStageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateID = table.Column<int>(type: "int", nullable: false),
                    StageID = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseWorkflowTemplateStages", x => x.TemplateStageID);
                    table.ForeignKey(
                        name: "FK_CaseWorkflowTemplateStages_CaseStages_StageID",
                        column: x => x.StageID,
                        principalTable: "CaseStages",
                        principalColumn: "StageID");
                    table.ForeignKey(
                        name: "FK_CaseWorkflowTemplateStages_CaseWorkflowTemplates_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "CaseWorkflowTemplates",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocumentRequirements_CaseID_DocumentTypeID",
                table: "CaseDocumentRequirements",
                columns: new[] { "CaseID", "DocumentTypeID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocumentRequirements_DocumentTypeID",
                table: "CaseDocumentRequirements",
                column: "DocumentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowStages_CaseID_SequenceNo",
                table: "CaseWorkflowStages",
                columns: new[] { "CaseID", "SequenceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowStages_CaseID_StageID",
                table: "CaseWorkflowStages",
                columns: new[] { "CaseID", "StageID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowStages_StageID",
                table: "CaseWorkflowStages",
                column: "StageID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowTemplateDocuments_DocumentTypeID",
                table: "CaseWorkflowTemplateDocuments",
                column: "DocumentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowTemplateDocuments_TemplateID_DocumentTypeID",
                table: "CaseWorkflowTemplateDocuments",
                columns: new[] { "TemplateID", "DocumentTypeID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowTemplates_CategoryID",
                table: "CaseWorkflowTemplates",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowTemplates_DefaultDepartmentID",
                table: "CaseWorkflowTemplates",
                column: "DefaultDepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowTemplates_FirmID_CategoryID",
                table: "CaseWorkflowTemplates",
                columns: new[] { "FirmID", "CategoryID" },
                unique: true,
                filter: "[FirmID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowTemplates_InitialStatusID",
                table: "CaseWorkflowTemplates",
                column: "InitialStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowTemplateStages_StageID",
                table: "CaseWorkflowTemplateStages",
                column: "StageID");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowTemplateStages_TemplateID_SequenceNo",
                table: "CaseWorkflowTemplateStages",
                columns: new[] { "TemplateID", "SequenceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowTemplateStages_TemplateID_StageID",
                table: "CaseWorkflowTemplateStages",
                columns: new[] { "TemplateID", "StageID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseDocumentRequirements");

            migrationBuilder.DropTable(
                name: "CaseWorkflowStages");

            migrationBuilder.DropTable(
                name: "CaseWorkflowTemplateDocuments");

            migrationBuilder.DropTable(
                name: "CaseWorkflowTemplateStages");

            migrationBuilder.DropTable(
                name: "CaseWorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "ArchiveReason",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ArchivedBy",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ArchivedDate",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "WorkflowTemplateID",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "WorkflowTemplateVersion",
                table: "Cases");
        }
    }
}
