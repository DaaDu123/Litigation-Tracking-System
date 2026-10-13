using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Cases.DTOs
{

    public class CreateCaseDTO
    {
        [Required(ErrorMessage = "Case Number is required")]
        [StringLength(100, ErrorMessage = "Case Number cannot exceed 100 characters")]
        public string CaseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Case Title is required")]
        [StringLength(255, ErrorMessage = "Case Title cannot exceed 255 characters")]
        public string CaseTitle { get; set; } = string.Empty;

        public string? CaseDescription { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Court is required")]
        public int CourtID { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Case Category is required")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [RegularExpression("^(High|Medium|Low)$", ErrorMessage = "Priority must be High, Medium, or Low")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Subject Matter is required")]
        [StringLength(255, ErrorMessage = "Subject Matter cannot exceed 255 characters")]
        public string SubjectMatter { get; set; } = string.Empty;

        [Required(ErrorMessage = "Filing Date is required")]
        public DateTime? FilingDate { get; set; }

        [Required(ErrorMessage = "Institution Date is required")]
        public DateTime? InstitutionDate { get; set; }

        [Required(ErrorMessage = "Registration Date is required")]
        public DateTime? RegistrationDate { get; set; }

        public DateTime? ExpectedDisposalDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Claimed Amount must be 0 or more")]
        public decimal ClaimedAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Potential Liability must be 0 or more")]
        public decimal PotentialLiability { get; set; } = 0;

        public string? FinancialImplication { get; set; }


        public int ResponsibleDepartmentID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Legal Officer is required")]
        public int CurrentLegalOfficerID { get; set; }
    }
}
