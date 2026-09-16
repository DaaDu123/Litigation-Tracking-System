using FluentValidation;

namespace LTSBackend.Features.Cases.Queries.GetCaseStatusHistory;

public class GetCaseStatusHistoryValidator : AbstractValidator<GetCaseStatusHistoryQuery>
{
    // Requires a valid CaseID.
    public GetCaseStatusHistoryValidator()
    {
        RuleFor(x => x.CaseID)
            .GreaterThan(0)
            .WithMessage("Valid Case ID is required");
    }
}
