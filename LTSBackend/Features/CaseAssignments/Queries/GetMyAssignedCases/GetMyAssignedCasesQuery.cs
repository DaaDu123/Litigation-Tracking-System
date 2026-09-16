using MediatR;
using LTSBackend.Features.CaseAssignments.DTOs;

namespace LTSBackend.Features.CaseAssignments.Queries.GetMyAssignedCases
{
    /// <summary>
    /// SRS: Section 5.10.2 Lawyer Dashboard - "My Cases Panel" / "View assigned cases"
    /// </summary>
    public class GetMyAssignedCasesQuery : IRequest<List<CaseAssignmentDetailDTO>>
    {
        public int UserID { get; set; }
    }
}
