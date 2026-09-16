using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Cases;

[Table("CaseMilestones")]
public class CaseMilestone
{
    [Key]
    public long MilestoneID { get; set; }

    [Required]
    public long CaseID { get; set; }

    [Required, MaxLength(255)]
    public string Milestone { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime MilestoneDate { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    public int CompletedBy { get; set; }
    public DateTime? CompletedDate { get; set; }

    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;
}