using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Masters;















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


    public int Version { get; set; } = 1;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public ICollection<CaseWorkflowTemplateStage> Stages { get; set; } = [];

    public ICollection<CaseWorkflowTemplateDocument> Documents { get; set; } = [];
}
