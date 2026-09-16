using FluentValidation;

namespace LTSBackend.Features.LoginHistory.DeleteAllOldHistory;

// Keeps the retention window sane: at least a day, at most ~10 years.
public class DeleteOldHistoryValidator : AbstractValidator<DeleteOldHistoryCommand>
{
    public DeleteOldHistoryValidator()
    {
        RuleFor(x => x.Days)
            .GreaterThan(0)
            .LessThanOrEqualTo(3650)
            .WithMessage("Days must be between 1 and 3650.");
    }
}