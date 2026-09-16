using MediatR;
using LTSBackend.Features.Milestones.DTOs;

namespace LTSBackend.Features.Milestones.Commands.CreateMilestone
{
    public class CreateMilestoneCommand : IRequest<long>
    {
        public CreateMilestoneDTO Milestone { get; set; } = null!;
    }
}
