using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Masters;

/// <summary>A required or optional Document Type of a Case Workflow Template (references the reusable DocumentType master).</summary>
[Table("CaseWorkflowTemplateDocuments")]
public class CaseWorkflowTemplateDocument
{
    [Key]
    public int TemplateDocumentID { get; set; }

    [Required]
    public int TemplateID { get; set; }

    [ForeignKey(nameof(TemplateID))]
    public CaseWorkflowTemplate Template { get; set; } = null!;

    [Required]
    public int DocumentTypeID { get; set; }

    [ForeignKey(nameof(DocumentTypeID))]
    public DocumentType DocumentType { get; set; } = null!;

    public bool IsRequired { get; set; }
}
