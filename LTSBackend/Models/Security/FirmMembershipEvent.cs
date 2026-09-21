using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

/// <summary>
/// Immutable history record of a firm-membership state transition
/// (Blocked / Unblocked / Removed / AutoReactivated). Kept separate from
/// the live state on <see cref="User"/> (MembershipStatus etc.) so that:
///   1. history survives even after User.FirmID is cleared on Remove;
///   2. "was this user ever blocked from THIS firm" can be answered even
///      after they've been removed and FirmID is null;
///   3. the AuditLogs table (generic, one line per command) doesn't have
///      to be the only place this is queryable from.
/// </summary>
[Table("FirmMembershipEvents")]
public class FirmMembershipEvent
{
    [Key]
    public long EventID { get; set; }

    public int FirmID { get; set; }

    public int UserID { get; set; }

    /// <summary>Blocked | Unblocked | Removed | AutoReactivated</summary>
    [Required, MaxLength(30)]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>Null for system-performed actions (e.g. AutoReactivated).</summary>
    public int? PerformedByUserID { get; set; }

    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
}
