using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.CaseAssignments.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseAssignments.Queries.GetCaseAssignments
{
    public class GetCaseAssignmentsHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService) : IRequestHandler<GetCaseAssignmentsQuery, List<CaseAssignmentDetailDTO>>
    {
        // =====================================================
        // HANDLE — lists a case's assignments, with visibility narrowed for non-full-visibility roles
        // A user without full case-directory visibility (e.g. Associate/
        // Moharrir/Intern) must actually be assigned to this case or gets
        // a 404 (not 403, so the case's existence isn't disclosed). Also
        // enforces firm isolation and resolves each assignment's
        // "assigned by" name for display.
        // =====================================================
        public async Task<List<CaseAssignmentDetailDTO>> Handle(GetCaseAssignmentsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);

                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                    {
                        throw new NotFoundException($"Case ID {request.CaseID} not found");
                    }
                }
            }

            var query = _context.CaseAssignments
                .AsNoTracking()
                .Include(a => a.Case)
                .Include(a => a.User)
                .Where(a => a.CaseID == request.CaseID);

            // Multi-tenant isolation - don't leak another firm's assignments
                query = query.Where(a => a.Case.FirmID == _currentUser.FirmID);

            if (request.ActiveOnly)
                query = query.Where(a => a.EndDate == null);

            var assignments = await query.OrderByDescending(a => a.AssignedDate).ToListAsync(cancellationToken);

            var assignedByIds = assignments.Select(a => a.AssignedBy).Distinct().ToList();
            var assignedByNames = await _context.Users
                .Where(u => assignedByIds.Contains(u.UserID))
                .ToDictionaryAsync(u => u.UserID, u => u.FullName, cancellationToken);

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
                AssignedByName = assignedByNames.TryGetValue(a.AssignedBy, out var n) ? n : null,
                EndDate = a.EndDate,
                Remarks = a.Remarks
            }).ToList();
        }
    }
}