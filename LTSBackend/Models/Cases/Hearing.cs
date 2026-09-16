using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Masters;

namespace LTSBackend.Models.Cases;

[Table("Hearings")]
public class Hearing
{
    [Key]
    public long HearingID { get; set; }

    [Required]
    public long CaseID { get; set; }

    [Required]
    public int CourtID { get; set; }

    [Required]
    public DateTime HearingDate { get; set; }

    [MaxLength(100)]
    public string? CourtRoom { get; set; }

    [MaxLength(150)]
    public string? JudgeName { get; set; }

    [MaxLength(255)]
    public string? Purpose { get; set; }

    [MaxLength(255)]
    public string? Outcome { get; set; }

    public DateTime? NextHearingDate { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;

    [ForeignKey(nameof(CourtID))]
    public Court Court { get; set; } = null!;

    public ICollection<HearingAttendance> HearingAttendances { get; set; } = [];
}