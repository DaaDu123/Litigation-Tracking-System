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

    /// <summary>Set when the user picked an existing court from the list. Leave 0 if typing a new one via CourtName.</summary>
    public int CourtID { get; set; }

    /// <summary>Set when the user typed a court name that isn't in the list yet - the server will create it.</summary>
    [StringLength(150)]
    public string? CourtName { get; set; }

    /// <summary>Set when the user picked an existing category from the list. Leave 0 if typing a new one via CategoryName.</summary>
    public int CategoryID { get; set; }

    /// <summary>Set when the user typed a category name that isn't in the list yet - the server will create it.</summary>
    [StringLength(150)]
    public string? CategoryName { get; set; }

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

    /// <summary>Set when the user picked an existing department from the list. Leave 0/null if typing a new one via DepartmentName.</summary>
    public int ResponsibleDepartmentID { get; set; }

    /// <summary>Set when the user typed a department name that isn't in the list yet - the server will create it.</summary>
    [StringLength(100)]
    public string? DepartmentName { get; set; }

    [Required(ErrorMessage = "Legal Officer is required")]
    public int CurrentLegalOfficerID { get; set; }
}
