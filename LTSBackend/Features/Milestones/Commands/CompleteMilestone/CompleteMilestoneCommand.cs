using MediatR;

namespace LTSBackend.Features.Milestones.Commands.CompleteMilestone
{
    public class CompleteMilestoneCommand : IRequest<bool>
    {
        public long MilestoneID { get; set; }
    }
}
