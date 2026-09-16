using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Comman.Enum;
namespace LTSBackend.Models.Security;

public class UserOtp
{
    [Key]
    public int OtpID { get; set; }
    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;
    [Required, MaxLength(6)]
    public string OtpCode { get; set; } = string.Empty;
    [Required]
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    [Required]
    public OtpPurpose Purpose { get; set; } = OtpPurpose.Registration;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UserID { get; set; }
    // Foreign Key & Navigation Property
    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
}