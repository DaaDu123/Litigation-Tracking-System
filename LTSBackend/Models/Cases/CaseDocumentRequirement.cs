using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Masters;

namespace LTSBackend.Models.Cases;









[Table("CaseDocumentRequirements")]
public class CaseDocumentRequirement
{
    [Key]
    public long RequirementID { get; set; }

    [Required]
    public long CaseID { get; set; }

    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;

    [Required]
    public int DocumentTypeID { get; set; }

    [ForeignKey(nameof(DocumentTypeID))]
    public DocumentType DocumentType { get; set; } = null!;

    public bool IsRequired { get; set; }
}
