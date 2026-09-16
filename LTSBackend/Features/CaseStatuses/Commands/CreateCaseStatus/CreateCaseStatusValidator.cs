using FluentValidation;

namespace LTSBackend.Features.CaseStatuses.Commands.CreateCaseStatus;

public class CreateCaseStatusValidator : AbstractValidator<CreateCaseStatusCommand>
{
    // Requires a status name, a non-negative sort SequenceNo, and a
    // well-formed hex ColorCode (e.g. #FF0000) for the status pill/badge UI.
    public CreateCaseStatusValidator()
    {
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
