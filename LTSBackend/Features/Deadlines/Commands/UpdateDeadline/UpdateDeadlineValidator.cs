using FluentValidation;

namespace LTSBackend.Features.Deadlines.Commands.UpdateDeadline
{
    public class UpdateDeadlineValidator : AbstractValidator<UpdateDeadlineCommand>
    {


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
