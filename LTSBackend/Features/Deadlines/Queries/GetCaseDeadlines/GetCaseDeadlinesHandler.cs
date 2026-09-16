using LTSBackend.Data;
using LTSBackend.Features.Deadlines.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Deadlines.Queries.GetCaseDeadlines
{
    public class GetCaseDeadlinesHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService) : IRequestHandler<GetCaseDeadlinesQuery, List<DeadlineDetailDTO>>
    {
        // =====================================================
        // HANDLE — lists a case's deadlines, scoped to who's allowed to see it
        // Same assignment-or-full-visibility rule as
        // GetCaseAssignmentsHandler (empty list, not an error, if denied),
        // plus firm isolation. Optionally filtered to only
        // completed/pending, and computes DaysRemaining/IsOverdue per
        // deadline for display.
        // =====================================================
        public async Task<List<DeadlineDetailDTO>> Handle(GetCaseDeadlinesQuery request, CancellationToken cancellationToken)
        {
            // SECURITY FIX (IDOR): firm scoping alone let any firm user - including
            // AssociateLawyer/Moharrir/InternParalegal - read deadlines for a case
            // they aren't assigned to. Mirrors GetCaseAssignmentsHandler.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        return new List<DeadlineDetailDTO>();
                }
            }

            var query = _context.Deadlines.AsNoTracking().Where(d => d.CaseID == request.CaseID);

            // Multi-tenant isolation
                query = query.Where(d => d.Case.FirmID == _currentUser.FirmID);

            if (request.Completed.HasValue)
                query = query.Where(d => d.Completed == request.Completed.Value);

            var deadlines = await query
                .Include(d => d.Case)
                .OrderBy(d => d.DueDate)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow.Date;

            return deadlines.Select(d => new DeadlineDetailDTO
            {
                DeadlineID = d.DeadlineID,
                CaseID = d.CaseID,
                CaseNumber = d.Case?.CaseNumber,
                CaseTitle = d.Case?.CaseTitle,
                DeadlineType = d.DeadlineType,
                DueDate = d.DueDate,
                ReminderDays = d.ReminderDays,
                Completed = d.Completed,
                CompletedDate = d.CompletedDate,
                Remarks = d.Remarks,
                DaysRemaining = (int)(d.DueDate.Date - now).TotalDays,
                IsOverdue = !d.Completed && d.DueDate.Date < now
            }).ToList();
        }
    }
}