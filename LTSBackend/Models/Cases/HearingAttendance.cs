using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Cases;

[Table("HearingAttendance")]
public class HearingAttendance
{
    [Key]
    public long AttendanceID { get; set; }

    [Required]
    public long HearingID { get; set; }

    [Required]
    public int UserID { get; set; }

    [MaxLength(100)]
    public string? AttendanceRole { get; set; }

    public bool Present { get; set; } = true;

    public DateTime? ArrivalTime { get; set; }

    public DateTime? DepartureTime { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    [ForeignKey(nameof(HearingID))]
    public Hearing Hearing { get; set; } = null!;

    [ForeignKey(nameof(UserID))]
    public User User { get; set; } = null!;
}