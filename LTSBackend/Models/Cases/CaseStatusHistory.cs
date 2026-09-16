using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Cases;

[Table("CaseStatusHistory")]
public class CaseStatusHistory
{
    [Key]
    public long HistoryID { get; set; }

    [Required]
    public long CaseID { get; set; }

    // OldStatusID is nullable because the first entry has no previous status
    public int? OldStatusID { get; set; }

    [Required]
    public int NewStatusID { get; set; }

    [Required]
    public int ChangedBy { get; set; }

    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? Remarks { get; set; }

    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;
}