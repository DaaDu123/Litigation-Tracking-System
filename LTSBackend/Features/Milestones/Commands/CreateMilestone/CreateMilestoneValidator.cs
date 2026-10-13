using FluentValidation;

namespace LTSBackend.Features.Milestones.Commands.CreateMilestone
{
    public class CreateMilestoneValidator : AbstractValidator<CreateMilestoneCommand>
    {


        public CreateMilestoneValidator()
        {
            RuleFor(x => x.Milestone).NotNull();
            RuleFor(x => x.Milestone.CaseID).GreaterThan(0);
            RuleFor(x => x.Milestone.Milestone).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Milestone.MilestoneDate).NotEmpty();
        }
    }
}
