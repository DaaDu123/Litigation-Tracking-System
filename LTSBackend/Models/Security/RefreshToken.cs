using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

public class RefreshToken
{
    [Key]
    public int RefreshTokenID { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public int UserID { get; set; }

    // Foreign Key & Navigation Property
    [ForeignKey(nameof(UserID))]
    public User User { get; set; } = null!;
}