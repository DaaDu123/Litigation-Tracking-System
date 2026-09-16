using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

public class LoginHistory
{
    [Key]
    public int LoginID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required]
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;

    public DateTime? LogoutTime { get; set; }

    [MaxLength(45)]
    public string? IPAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Success";

    public bool IsLoggedOut { get; set; } = false;


    // Foreign Key & Navigation Property
    [ForeignKey(nameof(UserID))]
    public User User { get; set; } = null!;
}