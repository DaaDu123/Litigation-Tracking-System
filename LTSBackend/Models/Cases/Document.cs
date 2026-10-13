using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Masters;

namespace LTSBackend.Models.Cases;

[Table("Documents")]
public class Document
{
    [Key]
    public long DocumentID { get; set; }

    [Required]
    public long CaseID { get; set; }

    [Required]
    public int DocumentTypeID { get; set; }

    [Required, MaxLength(255)]
    public string DocumentName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public int VersionNo { get; set; } = 1;
    public long FileSize { get; set; }
    public int UploadedBy { get; set; }
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    public bool IsLatest { get; set; } = true;

    [MaxLength(255)]
    public string? Remarks { get; set; }









    public bool IsDraft { get; set; } = false;



    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;

    [ForeignKey(nameof(DocumentTypeID))]
    public DocumentType DocumentType { get; set; } = null!;

    public ICollection<DocumentPermission> DocumentPermissions { get; set; } = [];
}