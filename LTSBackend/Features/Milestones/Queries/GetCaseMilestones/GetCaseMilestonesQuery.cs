using MediatR;
using LTSBackend.Features.Milestones.DTOs;

namespace LTSBackend.Features.Milestones.Queries.GetCaseMilestones
{
    public class GetCaseMilestonesQuery : IRequest<List<MilestoneDetailDTO>>
    {
        public long CaseID { get; set; }
    }
}
