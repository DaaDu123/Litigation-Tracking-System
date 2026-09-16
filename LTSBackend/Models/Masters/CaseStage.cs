using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Masters;

[Table("CaseStages")]
public class CaseStage
{
    [Key]
    public int StageID { get; set; }

    /// <summary>
    /// Same FirmID scoping pattern as Court/Department/CaseCategory - NULL
    /// = system-wide global stage, non-null = a firm's own custom stage.
    /// </summary>
    public int? FirmID { get; set; }

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [Required]
    [MaxLength(150)]
    public string StageName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
