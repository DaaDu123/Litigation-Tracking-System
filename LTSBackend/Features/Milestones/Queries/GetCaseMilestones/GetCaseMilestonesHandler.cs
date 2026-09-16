using LTSBackend.Data;
using LTSBackend.Features.Milestones.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Milestones.Queries.GetCaseMilestones
{
    public class GetCaseMilestonesHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService) : IRequestHandler<GetCaseMilestonesQuery, List<MilestoneDetailDTO>>
    {
        // =====================================================
        // HANDLE — lists a case's milestones, scoped to who's allowed to see it
        // Same assignment-or-full-visibility rule as
        // GetCaseAssignmentsHandler (empty list, not an error, if
        // denied), plus firm isolation. Resolves each completed
        // milestone's "completed by" name for display.
        // =====================================================
        public async Task<List<MilestoneDetailDTO>> Handle(GetCaseMilestonesQuery request, CancellationToken cancellationToken)
        {
            // SECURITY FIX (IDOR): firm scoping alone let any firm user - including
            // AssociateLawyer/Moharrir/InternParalegal - read milestones for a case
            // they aren't assigned to. Mirrors GetCaseAssignmentsHandler.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        return new List<MilestoneDetailDTO>();
                }
            }

            var query = _context.CaseMilestones
                .AsNoTracking()
                .Include(m => m.Case)
                .Where(m => m.CaseID == request.CaseID);

            // Multi-tenant isolation
                query = query.Where(m => m.Case.FirmID == _currentUser.FirmID);

            var milestones = await query
                .OrderBy(m => m.MilestoneDate)
                .ToListAsync(cancellationToken);

            var completedByIds = milestones.Where(m => m.CompletedDate != null).Select(m => m.CompletedBy).Distinct().ToList();
            var names = await _context.Users
                .Where(u => completedByIds.Contains(u.UserID))
                .ToDictionaryAsync(u => u.UserID, u => u.FullName, cancellationToken);

            return milestones.Select(m => new MilestoneDetailDTO
            {
                MilestoneID = m.MilestoneID,
                CaseID = m.CaseID,
                CaseNumber = m.Case?.CaseNumber,
                Milestone = m.Milestone,
                MilestoneDate = m.MilestoneDate,
                Description = m.Description,
                IsCompleted = m.CompletedDate != null,
                CompletedByName = m.CompletedDate != null && names.TryGetValue(m.CompletedBy, out var n) ? n : null,
                CompletedDate = m.CompletedDate
            }).ToList();
        }
    }
}