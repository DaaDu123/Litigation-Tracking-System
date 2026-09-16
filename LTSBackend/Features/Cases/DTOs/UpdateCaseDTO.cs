using System.ComponentModel.DataAnnotations;

namespace LTSBackend.Features.Cases.DTOs;

public class UpdateCaseDTO
{
    [Required]
    public long CaseID { get; set; }

    [StringLength(100)]
    public string? CaseNumber { get; set; }

    [StringLength(255)]
    public string? CaseTitle { get; set; }

    public string? CaseDescription { get; set; }

    public int? CourtID { get; set; }

    public int? CategoryID { get; set; }

    public int? StageID { get; set; }

    [RegularExpression("^(High|Medium|Low)$")]
    public string? Priority { get; set; }

    public string? SubjectMatter { get; set; }

    public DateTime? ExpectedDisposalDate { get; set; }

    public decimal? ClaimedAmount { get; set; }

    public decimal? PotentialLiability { get; set; }

    public int? CurrentLegalOfficerID { get; set; }

    public bool? IsArchived { get; set; }
}