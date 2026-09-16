using LTSBackend.Data;
using LTSBackend.Features.CaseAssignments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseAssignments.Queries.GetMyAssignedCases
{
    public class GetMyAssignedCasesHandler(AppDbContext _context) : IRequestHandler<GetMyAssignedCasesQuery, List<CaseAssignmentDetailDTO>>
    {
        // =====================================================
        // HANDLE — returns the given user's own currently-active case assignments
        // Used for the "My Cases" dashboard panel; only ever returns
        // assignments where EndDate is null (still active).
        // =====================================================
        public async Task<List<CaseAssignmentDetailDTO>> Handle(GetMyAssignedCasesQuery request, CancellationToken cancellationToken)
        {
            var assignments = await _context.CaseAssignments
                .AsNoTracking()
                .Include(a => a.Case)
                .Include(a => a.User)
                .Where(a => a.UserID == request.UserID && a.EndDate == null)
                .OrderByDescending(a => a.AssignedDate)
                .ToListAsync(cancellationToken);

            return assignments.Select(a => new CaseAssignmentDetailDTO
            {
                AssignmentID = a.AssignmentID,
                CaseID = a.CaseID,
                CaseNumber = a.Case?.CaseNumber,
                CaseTitle = a.Case?.CaseTitle,
                UserID = a.UserID,
                UserName = a.User?.FullName,
                UserEmail = a.User?.Email,
                AssignmentType = a.AssignmentType,
                LeadCounsel = a.LeadCounsel,
                AssignedDate = a.AssignedDate,
                EndDate = a.EndDate,
                Remarks = a.Remarks
            }).ToList();
        }
    }
}
