using MediatR;
using LTSBackend.Features.Deadlines.DTOs;

namespace LTSBackend.Features.Deadlines.Queries.GetUpcomingDeadlines
{





    public class GetUpcomingDeadlinesQuery : IRequest<List<DeadlineDetailDTO>>
    {
        public int? DaysAhead { get; set; }
    }
}
