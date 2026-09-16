using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Masters;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Cases;

[Table("Cases")]
public class Case
{
    [Key]
    public long CaseID { get; set; }

    [Required]
    public int FirmID { get; set; }

    [Required, MaxLength(50)]
    public string InternalReferenceNo { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string CaseNumber { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string CaseTitle { get; set; } = string.Empty;

    public string? CaseDescription { get; set; }

    [Required]
    public int CourtID { get; set; }

    [Required]
    public int CategoryID { get; set; }

    [Required]
    public int StatusID { get; set; }

    [Required]
    public int StageID { get; set; }

    [Required, MaxLength(20)]
    public string Priority { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string SubjectMatter { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime FilingDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime InstitutionDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime RegistrationDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime? ExpectedDisposalDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime? NextHearingDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime? UpcomingDeadline { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ClaimedAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PotentialLiability { get; set; }

    [MaxLength(255)]
    public string? FinancialImplication { get; set; }

    public int? ResponsibleDepartmentID { get; set; }

    public int? CurrentLegalOfficerID { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsArchived { get; set; } = false;

    public bool IsClosed { get; set; } = false;

    public DateTime? ClosureDate { get; set; }
    [ForeignKey(nameof(FirmID))]
    public Firm Firm { get; set; } = null!;

    [ForeignKey(nameof(CourtID))]
    public Court Court { get; set; } = null!;

    [ForeignKey(nameof(CategoryID))]
    public CaseCategory Category { get; set; } = null!;

    [ForeignKey(nameof(StatusID))]
    public CaseStatus Status { get; set; } = null!;

    [ForeignKey(nameof(StageID))]
    public CaseStage Stage { get; set; } = null!;
    [ForeignKey(nameof(ResponsibleDepartmentID))]
    public Department? Department { get; set; }

    [ForeignKey(nameof(CurrentLegalOfficerID))]
    public User? LegalOfficer { get; set; }

    [InverseProperty("Case")]
    public ICollection<CaseParty> CaseParties { get; set; } = [];

    [InverseProperty("Case")]
    public ICollection<Hearing> Hearings { get; set; } = [];

    [InverseProperty("Case")]
    public ICollection<Deadline> Deadlines { get; set; } = [];

    [InverseProperty("Case")]
    public ICollection<CaseAssignment> CaseAssignments { get; set; } = [];
}
