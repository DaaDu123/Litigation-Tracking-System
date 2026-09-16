using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Cases;

[Table("CaseNotes")]
public class CaseNote
{
    [Key]
    public long NoteID { get; set; }

    [Required]
    public long CaseID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required, MaxLength(50)]
    public string NoteType { get; set; } = string.Empty; // Internal, Confidential, General

    [Required]
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;

    [ForeignKey(nameof(UserID))]
    public User User { get; set; } = null!;
}