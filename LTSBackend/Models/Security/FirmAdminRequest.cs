using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

[Table("FirmAdminRequests")]
public class FirmAdminRequest
{
    [Key]
    public int RequestID { get; set; }









    [MaxLength(150)]
    public string? FirmName { get; set; }

    [MaxLength(30)]
    public string? FirmCode { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }





    [MaxLength(150)]
    public string? AdminFullName { get; set; }

    [Required, MaxLength(150)]
    public string AdminEmail { get; set; } = string.Empty;









    [MaxLength(255)]
    public string? AdminPasswordHash { get; set; }

    [MaxLength(20)]
    public string? AdminPhone { get; set; }










    public int? UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }



    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;


    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }


    public int? CreatedFirmID { get; set; }
}
