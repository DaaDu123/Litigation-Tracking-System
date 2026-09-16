using FluentValidation;

namespace LTSBackend.Features.Deadlines.Commands.CreateDeadline
{
    public class CreateDeadlineValidator : AbstractValidator<CreateDeadlineCommand>
    {
        // Requires a valid CaseID, a deadline type, and a Due Date that is
        // today or later (never in the past). ReminderDays (how many days
        // beforehand the user wants to be warned) must be 0-90 — see
        // ReminderService for how this is actually used, including its
        // guaranteed one-week floor.
        public CreateDeadlineValidator()
        {
            RuleFor(x => x.Deadline).NotNull();

            RuleFor(x => x.Deadline.CaseID)
                .GreaterThan(0).WithMessage("Case ID must be greater than 0");

            RuleFor(x => x.Deadline.DeadlineType)
                .NotEmpty().WithMessage("Deadline type is required")
                .MaximumLength(150);

            RuleFor(x => x.Deadline.DueDate)
                .NotEmpty().WithMessage("Due date is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Due date cannot be in the past");

            RuleFor(x => x.Deadline.ReminderDays)
                .GreaterThanOrEqualTo(0).WithMessage("Reminder days cannot be negative")
                .LessThanOrEqualTo(90).WithMessage("Reminder days cannot exceed 90");
        }
    }
}
