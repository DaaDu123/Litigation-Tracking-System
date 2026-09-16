using MediatR;
using LTSBackend.Features.CaseAssignments.DTOs;

namespace LTSBackend.Features.CaseAssignments.Queries.GetCaseAssignments
{
    public class GetCaseAssignmentsQuery : IRequest<List<CaseAssignmentDetailDTO>>
    {
        public long CaseID { get; set; }
        public bool ActiveOnly { get; set; } = false;
    }
}
