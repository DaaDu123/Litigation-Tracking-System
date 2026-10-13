using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Cases.Commands.UpdateCaseStatus;

public class UpdateCaseStatusHandler(AppDbContext _context, IAuditService _auditService, ILogger<UpdateCaseStatusHandler> _logger, IHttpContextAccessor _httpContextAccessor, ICurrentUserService _currentUser) : IRequestHandler<UpdateCaseStatusCommand, bool>
{









    public async Task<bool> Handle(UpdateCaseStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating case status: {CaseID}", request.CaseID);

        int currentUserId = GetCurrentUserId();


        var caseQuery = _context.Cases.Where(x => x.CaseID == request.CaseID);
            caseQuery = caseQuery.Where(x => x.FirmID == _currentUser.FirmID);
        var caseToUpdate = await caseQuery.FirstOrDefaultAsync(cancellationToken);

        if (caseToUpdate == null)
        {
            _logger.LogWarning("Case not found: {CaseID}", request.CaseID);
            throw new NotFoundException($"Case ID {request.CaseID} not found");
        }


        var newStatus = await _context.CaseStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.StatusID == request.NewStatusID, cancellationToken);

        if (newStatus == null)
        {
            _logger.LogWarning("Status not found: {StatusID}", request.NewStatusID);
            throw new NotFoundException($"Status ID {request.NewStatusID} not found");
        }


        if (caseToUpdate.StatusID == request.NewStatusID)
        {
            _logger.LogWarning("Attempting to set the same status: {CaseID}", request.CaseID);
            throw new ValidationException(new List<string>
            {
                "The new status is the same as the current status"
            });
        }


        int oldStatusID = caseToUpdate.StatusID;






        caseToUpdate.StatusID = request.NewStatusID;
        caseToUpdate.IsClosed = newStatus.IsClosed;

        if (newStatus.IsClosed)
        {

            caseToUpdate.ClosureDate ??= DateTime.UtcNow.Date;
        }
        else
        {

            caseToUpdate.ClosureDate = null;
        }

        caseToUpdate.ModifiedBy = currentUserId;
        caseToUpdate.ModifiedDate = DateTime.UtcNow;


        var statusHistory = new CaseStatusHistory
        {
            CaseID = request.CaseID,
            OldStatusID = oldStatusID,
            NewStatusID = request.NewStatusID,
            ChangedBy = currentUserId,
            ChangedDate = DateTime.UtcNow,
            Remarks = request.Remarks
        };

        _context.CaseStatusHistories.Add(statusHistory);


        var auditLog = _auditService.Create(
            currentUserId,
            $"Case Status Update: {caseToUpdate.CaseNumber} - {newStatus.StatusName}");

        _context.AuditLogs.Add(auditLog);


        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Case status successfully updated: {CaseID} from {OldStatus} to {NewStatus}",
            request.CaseID, oldStatusID, request.NewStatusID);

        return true;
    }







    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Case status update rejected: missing or invalid user identity claim");
            throw new UnauthorizedException("Unable to determine the current user's identity.");
        }

        return userId;
    }
}