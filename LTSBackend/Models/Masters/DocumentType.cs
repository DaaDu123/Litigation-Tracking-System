using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Masters;

[Table("DocumentTypes")]
public class DocumentType
{
    [Key]
    public int DocumentTypeID { get; set; }

    /// <summary>
    /// Same FirmID scoping pattern as Court/Department/CaseCategory - NULL
    /// = system-wide global document type, non-null = a firm's own custom type.
    /// </summary>
    public int? FirmID { get; set; }

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [Required]
    [MaxLength(160)]
    public string TypeName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
