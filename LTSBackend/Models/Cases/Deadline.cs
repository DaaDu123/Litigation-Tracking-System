using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Cases;

[Table("Deadlines")]
public class Deadline
{
    [Key]
    public long DeadlineID { get; set; }

    [Required]
    public long CaseID { get; set; }

    [Required, MaxLength(100)]
    public string DeadlineType { get; set; } = string.Empty;

    [Required, Column(TypeName = "date")]
    public DateTime DueDate { get; set; }

    public int ReminderDays { get; set; }
    public bool Completed { get; set; } = false;
    public DateTime? CompletedDate { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;
}