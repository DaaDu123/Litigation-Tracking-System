using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Masters;

namespace LTSBackend.Models.Cases;

/// <summary>Allowed values of <see cref="CaseWorkflowStage.Status"/>.</summary>
public static class CaseWorkflowStageState
{
    public const string Pending = "Pending";
    public const string Active = "Active";
    public const string Completed = "Completed";
}

/// <summary>
/// A Case's OWN progress record for one stage of its workflow. Generated
/// from the Case Workflow Template when the case is created. Moving a case
/// between stages only updates these rows - the shared CaseStage master is
/// never touched, so every case's workflow is independent.
/// </summary>
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
