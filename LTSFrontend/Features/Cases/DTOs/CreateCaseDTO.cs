using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Cases.DTOs
{
    /// <summary>Client-side form model for Create Case. Mirrors LTSBackend.Features.Cases.DTOs.CreateCaseDTO / CreateCaseValidator.</summary>
    public class CreateCaseDTO
    {
        [Required(ErrorMessage = "Case Number is required")]
        [StringLength(100, ErrorMessage = "Case Number cannot exceed 100 characters")]
        public string CaseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Case Title is required")]
        [StringLength(255, ErrorMessage = "Case Title cannot exceed 255 characters")]
        public string CaseTitle { get; set; } = string.Empty;

        public string? CaseDescription { get; set; }

        /// <summary>Set when an existing court is picked from the list. 0 means the user typed a new name in <see cref="CourtName"/> instead.</summary>
        public int CourtID { get; set; }

        /// <summary>Free-typed court name, used only when <see cref="CourtID"/> is 0 (new court not yet in the list).</summary>
        [StringLength(150)]
        public string? CourtName { get; set; }

        /// <summary>Set when an existing category is picked from the list. 0 means the user typed a new name in <see cref="CategoryName"/> instead.</summary>
        public int CategoryID { get; set; }

        /// <summary>Free-typed category name, used only when <see cref="CategoryID"/> is 0 (new category not yet in the list).</summary>
        [StringLength(150)]
        public string? CategoryName { get; set; }

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

        /// <summary>Set when an existing department is picked from the list. 0 means the user typed a new name in <see cref="DepartmentName"/> instead (optional field).</summary>
        public int ResponsibleDepartmentID { get; set; }

        /// <summary>Free-typed department name, used only when <see cref="ResponsibleDepartmentID"/> is 0 (new department not yet in the list).</summary>
        [StringLength(100)]
        public string? DepartmentName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Legal Officer is required")]
        public int CurrentLegalOfficerID { get; set; }
    }
}
