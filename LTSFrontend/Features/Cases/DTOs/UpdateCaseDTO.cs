using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Cases.DTOs
{
    /// <summary>Client-side form model for Edit Case. Mirrors LTSBackend.Features.Cases.DTOs.UpdateCaseDTO.</summary>
    public class UpdateCaseDTO
    {
        [Required]
        public long CaseID { get; set; }

        [StringLength(100, ErrorMessage = "Case Number cannot exceed 100 characters")]
        public string? CaseNumber { get; set; }

        [StringLength(255, ErrorMessage = "Case Title cannot exceed 255 characters")]
        public string? CaseTitle { get; set; }

        public string? CaseDescription { get; set; }

        public int? CourtID { get; set; }

        public int? CategoryID { get; set; }

        public int? StageID { get; set; }

        [RegularExpression("^(High|Medium|Low)$", ErrorMessage = "Priority must be High, Medium, or Low")]
        public string? Priority { get; set; }

        public string? SubjectMatter { get; set; }

        public DateTime? ExpectedDisposalDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Claimed Amount must be 0 or more")]
        public decimal? ClaimedAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Potential Liability must be 0 or more")]
        public decimal? PotentialLiability { get; set; }

        public int? CurrentLegalOfficerID { get; set; }

        public bool? IsArchived { get; set; }

        public static UpdateCaseDTO FromCaseDetails(long caseId, CaseDTO source) => new()
        {
            CaseID = caseId,
            CaseNumber = source.CaseNumber,
            CaseTitle = source.CaseTitle,
            CaseDescription = source.CaseDescription,
            CourtID = source.CourtID,
            CategoryID = source.CategoryID,
            StageID = source.StageID,
            Priority = source.Priority,
            SubjectMatter = source.SubjectMatter,
            ExpectedDisposalDate = source.ExpectedDisposalDate,
            ClaimedAmount = source.ClaimedAmount,
            PotentialLiability = source.PotentialLiability,
            CurrentLegalOfficerID = source.LegalOfficerID,
            IsArchived = source.IsArchived
        };
    }
}
