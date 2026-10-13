using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;











[Table("FirmMembershipEvents")]
public class FirmMembershipEvent
{
    [Key]
    public long EventID { get; set; }

    public int FirmID { get; set; }

    public int UserID { get; set; }


    [Required, MaxLength(30)]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }


    public int? PerformedByUserID { get; set; }

    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
}
