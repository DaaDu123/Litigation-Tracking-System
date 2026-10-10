using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Masters;

namespace LTSBackend.Models.Cases;

/// <summary>
/// One line of a Case's document checklist (required or optional),
/// generated from the Case Workflow Template at creation. It only says
/// "this case expects a document of this type" - actual uploaded files
/// stay in Documents (CaseID + DocumentTypeID). Whether a line is
/// fulfilled is derived from the Documents table, never stored, so it can
/// never drift out of sync.
/// </summary>
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
