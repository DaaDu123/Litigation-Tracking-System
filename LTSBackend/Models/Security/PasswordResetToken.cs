using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;









public class PasswordResetToken
{
    [Key]
    public int TokenID { get; set; }

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;


    [Required, MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public int UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
}
