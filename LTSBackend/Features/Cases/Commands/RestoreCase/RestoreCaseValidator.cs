using FluentValidation;

namespace LTSBackend.Features.Cases.Commands.RestoreCase;

public class RestoreCaseValidator : AbstractValidator<RestoreCaseCommand>
{
    public RestoreCaseValidator()
    {
        RuleFor(x => x.CaseID).GreaterThan(0).WithMessage("Valid Case ID is required");
    }
}
