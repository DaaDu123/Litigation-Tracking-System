using FluentValidation;

namespace LTSBackend.Features.Cases.Commands.UpdateCase;

public class UpdateCaseValidator : AbstractValidator<UpdateCaseCommand>
{
    // Every field is an optional partial update, so each rule only runs
    // when that field was actually supplied. Enforces length caps, a
    // High/Medium/Low Priority, positive IDs for Court/Category/Legal
    // Officer, and non-negative money amounts.
    public UpdateCaseValidator()
    {
        RuleFor(x => x.CaseID)
            .GreaterThan(0)
            .WithMessage("Valid Case ID is required");

        RuleFor(x => x.CaseNumber)
            .MaximumLength(100)
            .WithMessage("Case Number cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CaseNumber));

        RuleFor(x => x.CaseTitle)
            .MaximumLength(255)
            .WithMessage("Case Title cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.CaseTitle));

        RuleFor(x => x.SubjectMatter)
            .MaximumLength(255)
            .WithMessage("Subject Matter cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.SubjectMatter));

        RuleFor(x => x.Priority)
            .Must(x => x == "High" || x == "Medium" || x == "Low")
            .WithMessage("Priority can only be High, Medium, or Low")
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.CourtID)
            .GreaterThan(0)
            .WithMessage("Valid Court is required")
            .When(x => x.CourtID.HasValue);

        RuleFor(x => x.CategoryID)
            .GreaterThan(0)
            .WithMessage("Valid Category is required")
            .When(x => x.CategoryID.HasValue);

        RuleFor(x => x.CurrentLegalOfficerID)
            .GreaterThan(0)
            .WithMessage("Valid Legal Officer is required")
            .When(x => x.CurrentLegalOfficerID.HasValue);

        RuleFor(x => x.ClaimedAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Claimed Amount must be greater than or equal to 0")
            .When(x => x.ClaimedAmount.HasValue);

        RuleFor(x => x.PotentialLiability)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Potential Liability must be greater than or equal to 0")
            .When(x => x.PotentialLiability.HasValue);
    }
}