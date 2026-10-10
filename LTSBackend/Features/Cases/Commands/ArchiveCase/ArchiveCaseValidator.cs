using FluentValidation;

namespace LTSBackend.Features.Cases.Commands.ArchiveCase;

public class ArchiveCaseValidator : AbstractValidator<ArchiveCaseCommand>
{
    public ArchiveCaseValidator()
    {
        RuleFor(x => x.CaseID).GreaterThan(0).WithMessage("Valid Case ID is required");
        RuleFor(x => x.Reason).MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}
