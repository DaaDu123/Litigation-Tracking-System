using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Masters;

[Table("Courts")]
public class Court
{
    [Key]
    public int CourtID { get; set; }


















    public int? FirmID { get; set; }

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [Required]
    [MaxLength(150)]
    public string CourtName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CourtType { get; set; }

    [MaxLength(200)]
    public string? Jurisdiction { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
