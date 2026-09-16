using FluentValidation;

namespace LTSBackend.Features.Cases.Commands.CreateCase;

public class CreateCaseValidator : AbstractValidator<CreateCaseCommand>
{
    // Requires the mandatory case fields (number/title/priority/subject
    // matter/dates), a resolvable Court/Category (either an ID or a typed
    // name), and — importantly — Expected Disposal Date must be today or
    // later (never a past date) and after the Registration Date.
    public CreateCaseValidator()
    {
        RuleFor(x => x.CaseNumber)
            .NotEmpty()
            .WithMessage("Case Number is required")
            .MaximumLength(100)
            .WithMessage("Case Number cannot exceed 100 characters");

        RuleFor(x => x.CaseTitle)
            .NotEmpty()
            .WithMessage("Case Title is required")
            .MaximumLength(255)
            .WithMessage("Case Title cannot exceed 255 characters");

        RuleFor(x => x.SubjectMatter)
            .NotEmpty()
            .WithMessage("Subject Matter is required")
            .MaximumLength(255)
            .WithMessage("Subject Matter cannot exceed 255 characters");

        RuleFor(x => x.Priority)
            .NotEmpty()
            .WithMessage("Priority is required")
            .Must(x => x == "High" || x == "Medium" || x == "Low")
            .WithMessage("Priority can only be High, Medium, or Low");

        RuleFor(x => x)
            .Must(x => x.CourtID > 0 || !string.IsNullOrWhiteSpace(x.CourtName))
            .WithMessage("Select a Court or type a new one")
            .OverridePropertyName("CourtID");

        RuleFor(x => x)
            .Must(x => x.CategoryID > 0 || !string.IsNullOrWhiteSpace(x.CategoryName))
            .WithMessage("Select a Category or type a new one")
            .OverridePropertyName("CategoryID");

        RuleFor(x => x.ResponsibleDepartmentID)
            .GreaterThan(0)
            .WithMessage("A valid Department is required — not 0 or a negative ID")
            .When(x => x.ResponsibleDepartmentID.HasValue && string.IsNullOrWhiteSpace(x.DepartmentName));

        RuleFor(x => x.CurrentLegalOfficerID)
            .GreaterThan(0)
            .WithMessage("A valid Legal Officer is required — not 0 or a negative ID")
            .When(x => x.CurrentLegalOfficerID.HasValue);

        RuleFor(x => x.FilingDate)
            .NotEmpty()
            .WithMessage("Filing Date is required")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Filing Date must be today or earlier");

        RuleFor(x => x.InstitutionDate)
            .NotEmpty()
            .WithMessage("Institution Date is required")
            .GreaterThanOrEqualTo(x => x.FilingDate)
            .WithMessage("Institution Date must be on or after the Filing Date");

        RuleFor(x => x.RegistrationDate)
            .NotEmpty()
            .WithMessage("Registration Date is required")
            .GreaterThanOrEqualTo(x => x.InstitutionDate)
            .WithMessage("Registration Date must be on or after the Institution Date");

        RuleFor(x => x.ExpectedDisposalDate)
            .GreaterThanOrEqualTo(DateTime.Now.Date)
            .WithMessage("Expected Disposal Date cannot be in the past")
            .GreaterThan(x => x.RegistrationDate)
            .WithMessage("Expected Disposal Date must be after the Registration Date")
            .When(x => x.ExpectedDisposalDate.HasValue);

        RuleFor(x => x.ClaimedAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Claimed Amount must be greater than or equal to 0");

        RuleFor(x => x.PotentialLiability)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Potential Liability must be greater than or equal to 0");
    }
}
