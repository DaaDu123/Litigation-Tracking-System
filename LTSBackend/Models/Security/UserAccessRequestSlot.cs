using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

/// <summary>
/// The single "access-request slot" every user owns. A normal user may have
/// only ONE active access request in total: either one request to a
/// FirmAdmin (<see cref="UserJoinRequest"/>) OR one request to the
/// SuperAdmin (<see cref="FirmAdminRequest"/> sent from the account) -
/// never both, never two of the same kind.
///
/// Those two requests live in different tables, so a cross-table UNIQUE
/// constraint is impossible. This table is the database-level guarantee:
/// UserID is the PRIMARY KEY, so SQL Server itself rejects a second slot for
/// the same user even if two requests are submitted at the exact same
/// instant. A row is inserted in the SAME transaction/SaveChanges as the
/// request it guards and removed in the same SaveChanges that moves that
/// request out of Pending (Approved / Rejected / Cancelled).
/// </summary>
[Table("UserAccessRequestSlots")]
public class UserAccessRequestSlot
{
    public const string TargetFirmAdmin = "FirmAdmin";
    public const string TargetSuperAdmin = "SuperAdmin";

    /// <summary>PK and FK to Users: at most one slot per user (not identity - the user's own ID).</summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int UserID { get; set; }

    /// <summary>FirmAdmin | SuperAdmin - which kind of request currently occupies the slot.</summary>
    [Required, MaxLength(20)]
    public string TargetType { get; set; } = TargetFirmAdmin;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
}
