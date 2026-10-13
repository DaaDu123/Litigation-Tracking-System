using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Masters;


[Table("CaseWorkflowTemplateStages")]
public class CaseWorkflowTemplateStage
{
    [Key]
    public int TemplateStageID { get; set; }

    [Required]
    public int TemplateID { get; set; }

    [ForeignKey(nameof(TemplateID))]
    public CaseWorkflowTemplate Template { get; set; } = null!;

    [Required]
    public int StageID { get; set; }

    [ForeignKey(nameof(StageID))]
    public CaseStage Stage { get; set; } = null!;


    [Required]
    public int SequenceNo { get; set; }
}
