using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Masters;

[Table("CaseStatus")]
public class CaseStatus
{
    [Key]
    public int StatusID { get; set; }

    /// <summary>
    /// Same FirmID scoping pattern as Court/Department/CaseCategory - NULL
    /// = system-wide global status, non-null = a firm's own custom status.
    /// </summary>
    public int? FirmID { get; set; }

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [Required]
    [MaxLength(100)]
    public string StatusName { get; set; } = string.Empty;

    [Required]
    public int SequenceNo { get; set; }

    [Required]
    [MaxLength(10)]
    public string ColorCode { get; set; } = string.Empty;

    [Required]
    public bool IsClosed { get; set; } = false;

    public bool IsActive { get; set; } = true;
}
