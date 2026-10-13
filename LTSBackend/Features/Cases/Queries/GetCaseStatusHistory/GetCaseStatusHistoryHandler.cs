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







    public async Task<List<CaseStatusHistoryDTO>> Handle(GetCaseStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching status history for case: {CaseID}", request.CaseID);





        var caseExists = await _context.Cases.AsNoTracking().AnyAsync(x => x.CaseID == request.CaseID && x.FirmID == _currentUser.FirmID, cancellationToken);

        if (!caseExists)
        {
            _logger.LogWarning("Status history requested for missing/foreign case: {CaseID}", request.CaseID);
            throw new NotFoundException($"Case ID {request.CaseID} not found");
        }





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
