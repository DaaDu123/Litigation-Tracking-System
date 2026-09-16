using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Cases;

[Table("CaseParties")]
public class CaseParty
{
    [Key]
    public long PartyID { get; set; }

    [Required]
    public long CaseID { get; set; }

    [Required, MaxLength(20)]
    public string PartyType { get; set; } = string.Empty; // Plaintiff, Defendant, Petitioner, etc.

    [Required, MaxLength(255)]
    public string PartyName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Organization { get; set; }

    [MaxLength(50)]
    public string? CNIC { get; set; }

    [MaxLength(50)]
    public string? NTN { get; set; }

    [MaxLength(20)]
    public string? ContactNo { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(255)]
    public string? LawyerName { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;
}