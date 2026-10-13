using FluentValidation;

namespace LTSBackend.Features.Milestones.Commands.UpdateMilestone
{
    public class UpdateMilestoneValidator : AbstractValidator<UpdateMilestoneCommand>
    {


        public UpdateMilestoneValidator()
        {
            RuleFor(x => x.Milestone).NotNull();
            RuleFor(x => x.Milestone.MilestoneID).GreaterThan(0);
            RuleFor(x => x.Milestone.Milestone).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Milestone.MilestoneDate).NotEmpty();
        }
    }
}
