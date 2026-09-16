using FluentValidation;

namespace LTSBackend.Features.Milestones.Commands.UpdateMilestone
{
    public class UpdateMilestoneValidator : AbstractValidator<UpdateMilestoneCommand>
    {
        // Requires a valid MilestoneID plus the same name/date rules as
        // CreateMilestoneValidator.
        public UpdateMilestoneValidator()
        {
            RuleFor(x => x.Milestone).NotNull();
            RuleFor(x => x.Milestone.MilestoneID).GreaterThan(0);
            RuleFor(x => x.Milestone.Milestone).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Milestone.MilestoneDate).NotEmpty();
        }
    }
}
