using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Audit;

public class AuditLog
{
    [Key]
    public int LogID { get; set; }
    public int? UserID { get; set; }
    [Required]
    [MaxLength(255)]
    public string Action { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? IPAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    // Foreign Key & Navigation Property (Lazy-loaded if needed)
    [ForeignKey(nameof(UserID))]
    public Models.Security.User? User { get; set; }
    public string? TableName { get; set; }

    public int? RecordID { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Description { get; set; }
}