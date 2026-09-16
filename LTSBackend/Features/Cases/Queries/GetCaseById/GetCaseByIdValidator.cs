using FluentValidation;

namespace LTSBackend.Features.Cases.Queries.GetCaseById;

public class GetCaseByIdValidator : AbstractValidator<GetCaseByIdQuery>
{
    // Requires a valid CaseID.
    public GetCaseByIdValidator()
    {
        RuleFor(x => x.CaseID)
            .GreaterThan(0)
            .WithMessage("Valid Case ID is required");
    }
}