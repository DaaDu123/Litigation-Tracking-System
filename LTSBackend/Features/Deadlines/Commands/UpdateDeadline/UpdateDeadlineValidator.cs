using FluentValidation;

namespace LTSBackend.Features.Deadlines.Commands.UpdateDeadline
{
    public class UpdateDeadlineValidator : AbstractValidator<UpdateDeadlineCommand>
    {
        // Requires a valid DeadlineID plus the same type/due-date/ReminderDays
        // rules as CreateDeadlineValidator.
        public UpdateDeadlineValidator()
        {
            RuleFor(x => x.Deadline).NotNull();
            RuleFor(x => x.Deadline.DeadlineID).GreaterThan(0);
            RuleFor(x => x.Deadline.DeadlineType).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Deadline.DueDate).NotEmpty();
            RuleFor(x => x.Deadline.ReminderDays).GreaterThanOrEqualTo(0).LessThanOrEqualTo(90);
        }
    }
}
