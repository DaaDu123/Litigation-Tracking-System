using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;







[Table("Firms")]
public class Firm
{
    [Key]
    public int FirmID { get; set; }

    [Required, MaxLength(150)]
    public string FirmName { get; set; } = string.Empty;






    [Required, MaxLength(30)]
    public string FirmCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }


    [MaxLength(150)]
    public string? CustomDomain { get; set; }






    [MaxLength(30)]
    public string MigrationStatus { get; set; } = "None";

    public DateTime? MigrationRequestedAt { get; set; }
    public int? MigrationRequestedBy { get; set; }
    public DateTime? MigrationCompletedAt { get; set; }

    [MaxLength(500)]
    public string? MigrationNotes { get; set; }

    public bool IsBlocked { get; set; } = false;
    [MaxLength(255)]
    public string? BlockedReason { get; set; }
    public DateTime? BlockedAt { get; set; }
    public int? BlockedBy { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }


    public ICollection<User> Users { get; set; } = [];
}