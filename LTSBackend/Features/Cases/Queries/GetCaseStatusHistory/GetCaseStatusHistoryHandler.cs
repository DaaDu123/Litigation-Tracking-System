using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Cases.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Cases.Queries.GetCaseStatusHistory;

public class GetCaseStatusHistoryHandler(AppDbContext _context,ICurrentUserService _currentUser,IPermissionService _permissionService,
    ILogger<GetCaseStatusHistoryHandler> _logger): IRequestHandler<GetCaseStatusHistoryQuery, List<CaseStatusHistoryDTO>>
{
    // =====================================================
    // HANDLE — read side of FR-05, returns a case's status timeline
    // Same firm + assignment visibility rule as GetCaseByIdHandler, then
    // returns every CaseStatusHistory row (old status, new status, who
    // changed it, when, remarks) newest-first, with status/user names
    // resolved for display.
    // =====================================================
    public async Task<List<CaseStatusHistoryDTO>> Handle(GetCaseStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching status history for case: {CaseID}", request.CaseID);

        // 1. Confirm the case exists and is within the caller's firm.
        // (Same tenant check as GetCaseByIdHandler - AppDbContext's
        // global query filter already scopes Cases by FirmID, but we
        // check explicitly here too so the 404 message is accurate.)
        var caseExists = await _context.Cases.AsNoTracking().AnyAsync(x => x.CaseID == request.CaseID && x.FirmID == _currentUser.FirmID, cancellationToken);

        if (!caseExists)
        {
            _logger.LogWarning("Status history requested for missing/foreign case: {CaseID}", request.CaseID);
            throw new NotFoundException($"Case ID {request.CaseID} not found");
        }

        // 1b. Same per-case visibility rule as GetCaseByIdHandler: users
        // without full case-directory visibility must be actively
        // assigned to this case to see anything about it, including
        // its status history. 404 (not 403) so existence isn't leaked.
        if (_currentUser.UserID.HasValue)
        {
            bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);

            if (!hasFullVisibility)
            {
                bool isAssigned = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.CaseID, cancellationToken);

                if (!isAssigned)
                {
                    _logger.LogWarning("Access denied: User {UserId} is not assigned to case {CaseID}",_currentUser.UserID.Value,request.CaseID);
                    throw new NotFoundException($"Case ID {request.CaseID} not found");
                }
            }
        }

        // 2. Pull the timeline, newest first, with names resolved
        // (old status can be null for the very first "New" entry).
        var history = await _context.CaseStatusHistories
            .AsNoTracking()
            .Where(x => x.CaseID == request.CaseID)
            .OrderByDescending(x => x.ChangedDate)
            .Select(x => new CaseStatusHistoryDTO
            {
                HistoryID = x.HistoryID,
                CaseID = x.CaseID,
                OldStatusID = x.OldStatusID,
                OldStatusName = x.OldStatusID == null
                    ? null
                    : _context.CaseStatuses.Where(s => s.StatusID == x.OldStatusID).Select(s => s.StatusName).FirstOrDefault(),
                NewStatusID = x.NewStatusID,
                NewStatusName = _context.CaseStatuses.Where(s => s.StatusID == x.NewStatusID).Select(s => s.StatusName).FirstOrDefault() ?? "Unknown",
                ChangedBy = x.ChangedBy,
                ChangedByName = _context.Users.Where(u => u.UserID == x.ChangedBy).Select(u => u.FullName).FirstOrDefault() ?? "Unknown",
                ChangedDate = x.ChangedDate,
                Remarks = x.Remarks
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Fetched {Count} status history entries for case {CaseID}", history.Count, request.CaseID);

        return history;
    }
}
