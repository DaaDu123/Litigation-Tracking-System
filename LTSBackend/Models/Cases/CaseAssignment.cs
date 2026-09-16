using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Cases;

[Table("CaseAssignments")]
public class CaseAssignment
{
    [Key]
    public long AssignmentID { get; set; }
    [Required]
    public long CaseID { get; set; }
    [Required]
    public int UserID { get; set; }
    [Required, MaxLength(30)]
    public string AssignmentType { get; set; } = string.Empty;
    [Column("IsLeadCounsel")]                       // ✅ maps to actual DB column name
    public bool LeadCounsel { get; set; } = false;
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public int AssignedBy { get; set; }
    public DateTime? EndDate { get; set; }
    [MaxLength(255)]
    public string? Remarks { get; set; }
    // Navigation Properties
    [ForeignKey(nameof(CaseID))]
    public Case Case { get; set; } = null!;
    [ForeignKey(nameof(UserID))]
    public User User { get; set; } = null!;
}