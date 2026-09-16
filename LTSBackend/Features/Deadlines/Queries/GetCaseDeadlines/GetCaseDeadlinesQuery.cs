using MediatR;
using LTSBackend.Features.Deadlines.DTOs;

namespace LTSBackend.Features.Deadlines.Queries.GetCaseDeadlines
{
    public class GetCaseDeadlinesQuery : IRequest<List<DeadlineDetailDTO>>
    {
        public long CaseID { get; set; }
        public bool? Completed { get; set; }
    }
}
