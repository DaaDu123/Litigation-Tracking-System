using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

/// <summary>
/// Backs the "forgot password" email-link flow (replaces the old
/// 6-digit-OTP-based password reset). A single-use, time-limited token is
/// generated, emailed to the user as a clickable link, and only its
/// SHA-256 hash is ever persisted - the raw token exists only in the
/// email and briefly in memory, never in the database or logs, so a
/// database leak alone cannot be used to reset anyone's password.
/// </summary>
public class PasswordResetToken
{
    [Key]
    public int TokenID { get; set; }

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <summary>SHA-256 hash (hex) of the raw token sent in the email link.</summary>
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
