using FluentValidation;
using LTSBackend.Features.LoginHistory.GetAllLoginHistory;

namespace LTSBackend.Features.LoginHistory.Queries.GetAllLoginHistory;

public class GetAllLoginHistoryValidator : AbstractValidator<GetAllLoginHistoryQuery>
{
    // Requires sane pagination and, if both a FromDate and ToDate are
    // supplied, that FromDate isn't after ToDate.
    public GetAllLoginHistoryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x)
            .Must(x =>
                !x.FromDate.HasValue ||
                !x.ToDate.HasValue ||
                x.FromDate <= x.ToDate)
            .WithMessage("FromDate cannot be greater than ToDate.");
    }
}