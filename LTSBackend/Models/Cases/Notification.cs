using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Cases;

[Table("Notifications")]
public class Notification
{
    [Key]
    public long NotificationID { get; set; }
    [Required]
    public int NotificationTypeID { get; set; }
    [Required]
    public int UserID { get; set; }
    public long? CaseID { get; set; }
    [MaxLength(300)]
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadDate { get; set; }
    public bool IsSent { get; set; } = false;
    public DateTime? SentDate { get; set; }
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium";   // Low | Medium | High | Critical
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    // Foreign Keys & Navigation
    [ForeignKey(nameof(NotificationTypeID))]
    public NotificationType? NotificationType { get; set; }
    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
    [ForeignKey(nameof(CaseID))]
    public Case? Case { get; set; }
}