using FluentValidation;

namespace LTSBackend.Features.CaseStatuses.Commands.UpdateCaseStatus;

public class UpdateCaseStatusValidator : AbstractValidator<UpdateCaseStatusCommand>
{
    // Requires a valid StatusID plus the same name/sequence/colour rules as
    // CreateCaseStatusValidator.
    public UpdateCaseStatusValidator()
    {
        RuleFor(x => x.StatusID).GreaterThan(0).WithMessage("Valid status ID is required.");

        RuleFor(x => x.StatusName)
            .NotEmpty().WithMessage("Status name is required.")
            .MaximumLength(100).WithMessage("Status name cannot exceed 100 characters.");

        RuleFor(x => x.SequenceNo)
            .GreaterThanOrEqualTo(0).WithMessage("Sequence number must be zero or greater.");

        RuleFor(x => x.ColorCode)
            .NotEmpty().WithMessage("Color code is required.")
            .MaximumLength(10).WithMessage("Color code cannot exceed 10 characters.")
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Color code must be a valid hex color, e.g. #FF0000.");
    }
}
