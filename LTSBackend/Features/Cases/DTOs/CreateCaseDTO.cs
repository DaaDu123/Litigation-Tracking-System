using System.ComponentModel.DataAnnotations;

namespace LTSBackend.Features.Cases.DTOs;

public class CreateCaseDTO
{
    [Required(ErrorMessage = "Case Number is required")]
    [StringLength(100)]
    public string CaseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Case Title is required")]
    [StringLength(255)]
    public string CaseTitle { get; set; } = string.Empty;

    public string? CaseDescription { get; set; }

    /// <summary>The Court picked from the firm's configured courts (required). Courts are created once in Master Data, never from a case.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Court is required")]
    public int CourtID { get; set; }

    /// <summary>The Case Category / Case Type (required). Its workflow template drives status, stages, department and document checklist.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Case Category is required")]
    public int CategoryID { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    [RegularExpression("^(High|Medium|Low)$")]
    public string Priority { get; set; } = "Medium";

    [Required(ErrorMessage = "Subject Matter is required")]
    [StringLength(255)]
    public string SubjectMatter { get; set; } = string.Empty;

    [Required(ErrorMessage = "Filing Date is required")]
    public DateTime FilingDate { get; set; }

    [Required(ErrorMessage = "Institution Date is required")]
    public DateTime InstitutionDate { get; set; }

    [Required(ErrorMessage = "Registration Date is required")]
    public DateTime RegistrationDate { get; set; }

    public DateTime? ExpectedDisposalDate { get; set; }

    public decimal ClaimedAmount { get; set; } = 0;

    public decimal PotentialLiability { get; set; } = 0;

    public string? FinancialImplication { get; set; }

    /// <summary>Optional override. Leave null to use the category template's default department.</summary>
    public int? ResponsibleDepartmentID { get; set; }

    [Required(ErrorMessage = "Legal Officer is required")]
    public int CurrentLegalOfficerID { get; set; }
}
