using FluentValidation;
namespace LTSBackend.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsValidator : AbstractValidator<GetAuditLogsQuery>
{
    // Requires sane pagination and, if both a FromDate and ToDate are
    // supplied, that FromDate isn't after ToDate.
    public GetAuditLogsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x)
            .Must(x =>
                !x.FromDate.HasValue ||
                !x.ToDate.HasValue ||
                x.FromDate <= x.ToDate)
            .WithMessage("From date must be less than or equal to to date.");
    }
}