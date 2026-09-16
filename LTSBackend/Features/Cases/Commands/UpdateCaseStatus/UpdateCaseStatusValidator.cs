using FluentValidation;

namespace LTSBackend.Features.Cases.Commands.UpdateCaseStatus;

public class UpdateCaseStatusValidator : AbstractValidator<UpdateCaseStatusCommand>
{
    // Requires a valid CaseID and NewStatusID, with an optional
    // length-capped Remarks.
    public UpdateCaseStatusValidator()
    {
        RuleFor(x => x.CaseID)
            .GreaterThan(0)
            .WithMessage("Valid Case ID is required");

        RuleFor(x => x.NewStatusID)
            .GreaterThan(0)
            .WithMessage("Valid Status is required");

        RuleFor(x => x.Remarks)
            .MaximumLength(1000)
            .WithMessage("Remarks cannot exceed 1000 characters");
    }
}