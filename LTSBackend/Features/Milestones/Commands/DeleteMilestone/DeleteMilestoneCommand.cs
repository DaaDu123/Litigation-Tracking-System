using MediatR;

namespace LTSBackend.Features.Milestones.Commands.DeleteMilestone
{
    public class DeleteMilestoneCommand : IRequest<bool>
    {
        public long MilestoneID { get; set; }
    }
}
