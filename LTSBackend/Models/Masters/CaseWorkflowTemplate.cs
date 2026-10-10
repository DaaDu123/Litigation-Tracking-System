using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Masters;

/// <summary>
/// Master configuration that drives Case creation: one template per Case
/// Category (per scope). Defines the default Department, the initial
/// Status, the ordered Stages and the required/optional Document Types
/// that every new Case of that category starts with.
///
/// Same FirmID scoping pattern as the other masters - NULL = global
/// (SuperAdmin-managed) template shared by every firm, non-null = a firm's
/// own template. A firm template for a category overrides the global one
/// for that firm. A template only REFERENCES reusable master rows
/// (Department/Status/Stage/DocumentType) - it never duplicates them, and
/// Cases never point back at it (Cases receive their own copies of the
/// stage/document rows so later template edits never rewrite history).
/// </summary>
[Table("CaseWorkflowTemplates")]
public class CaseWorkflowTemplate
{
    [Key]
    public int TemplateID { get; set; }

    public int? FirmID { get; set; }

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [Required]
    public int CategoryID { get; set; }

    [ForeignKey(nameof(CategoryID))]
    public CaseCategory Category { get; set; } = null!;

    public int? DefaultDepartmentID { get; set; }

    [ForeignKey(nameof(DefaultDepartmentID))]
    public Department? DefaultDepartment { get; set; }

    [Required]
    public int InitialStatusID { get; set; }

    [ForeignKey(nameof(InitialStatusID))]
    public CaseStatus InitialStatus { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    /// <summary>Incremented on every save so cases can record which revision they were built from.</summary>
    public int Version { get; set; } = 1;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public ICollection<CaseWorkflowTemplateStage> Stages { get; set; } = [];

    public ICollection<CaseWorkflowTemplateDocument> Documents { get; set; } = [];
}
