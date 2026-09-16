using LTSBackend.Models.Cases;
using System.ComponentModel.DataAnnotations;

namespace LTSBackend.Models.Security;

public class NotificationType
{
    [Key]
    public int NotificationTypeID { get; set; }

    [Required, MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsEmail { get; set; } = true;
    public bool IsSMS { get; set; } = false;
    public bool IsInApp { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public ICollection<Notification> Notifications { get; set; } = [];
}