using MediatR;
using LTSBackend.Features.Milestones.DTOs;

namespace LTSBackend.Features.Milestones.Commands.UpdateMilestone
{
    public class UpdateMilestoneCommand : IRequest<bool>
    {
        public UpdateMilestoneDTO Milestone { get; set; } = null!;
    }
}
