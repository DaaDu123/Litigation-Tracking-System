using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;
















[Table("UserAccessRequestSlots")]
public class UserAccessRequestSlot
{
    public const string TargetFirmAdmin = "FirmAdmin";
    public const string TargetSuperAdmin = "SuperAdmin";


    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int UserID { get; set; }


    [Required, MaxLength(20)]
    public string TargetType { get; set; } = TargetFirmAdmin;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
}
