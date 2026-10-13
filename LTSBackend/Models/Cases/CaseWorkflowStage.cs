using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Masters;

namespace LTSBackend.Models.Cases;


public static class CaseWorkflowStageState
{
    public const string Pending = "Pending";
    public const string Active = "Active";
    public const string Completed = "Completed";
}







[Table("CaseWorkflowStages")]
public class CaseWorkflowStage
{
    [Key]
    public long CaseWorkflowStageID { get; set; }

    [Required]
    public long CaseID { get; set; }

    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;

    [Required]
    public int StageID { get; set; }

    [ForeignKey(nameof(StageID))]
    public CaseStage Stage { get; set; } = null!;

    [Required]
    public int SequenceNo { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = CaseWorkflowStageState.Pending;

    public DateTime? StartedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
