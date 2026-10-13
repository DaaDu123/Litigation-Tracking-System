using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;














[Table("UserJoinRequests")]
public class UserJoinRequest
{
    [Key]
    public int RequestID { get; set; }


    public int FirmID { get; set; }








    public int? UserID { get; set; }






    [MaxLength(150)]
    public string? FullName { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }


    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }






    public int RequestedRoleID { get; set; }



    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;


    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }


    public int? CreatedUserID { get; set; }


    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
}
