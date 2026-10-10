using FluentValidation;

namespace LTSBackend.Features.Cases.Commands.DeleteCase;

public class DeleteCaseValidator : AbstractValidator<DeleteCaseCommand>
{
    // Permanent deletion needs a valid CaseID, the typed case number and a reason.
    public DeleteCaseValidator()
    {
        RuleFor(x => x.CaseID).GreaterThan(0).WithMessage("Valid Case ID is required");
        RuleFor(x => x.ConfirmCaseNumber).NotEmpty().WithMessage("Type the case number to confirm permanent deletion");
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A reason is required for permanent deletion")
            .MinimumLength(5).WithMessage("Reason must be at least 5 characters")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}
