using MediatR;
using LTSBackend.Features.CaseAssignments.DTOs;

namespace LTSBackend.Features.CaseAssignments.Queries.GetMyAssignedCases
{



    public class GetMyAssignedCasesQuery : IRequest<List<CaseAssignmentDetailDTO>>
    {
        public int UserID { get; set; }
    }
}
