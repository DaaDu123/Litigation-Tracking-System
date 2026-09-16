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
    // =====================================================
    // HANDLE — changes a case's status and records the transition (FR-05)
    // Firm-scoped lookup, rejects setting the same status again, and syncs
    // Case.IsClosed/ClosureDate with the new status's IsClosed flag (so
    // vw_ActiveCases/vw_ClosedCases stay accurate) — clearing ClosureDate
    // again if the case is re-opened. Writes a CaseStatusHistory row
    // (old status, new status, who changed it, remarks) and an audit log
    // entry.
    // =====================================================
    public async Task<bool> Handle(UpdateCaseStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating case status: {CaseID}", request.CaseID);

        int currentUserId = GetCurrentUserId();

        // 1. Find Case (firm-scoped)
        var caseQuery = _context.Cases.Where(x => x.CaseID == request.CaseID);
            caseQuery = caseQuery.Where(x => x.FirmID == _currentUser.FirmID);
        var caseToUpdate = await caseQuery.FirstOrDefaultAsync(cancellationToken);

        if (caseToUpdate == null)
        {
            _logger.LogWarning("Case not found: {CaseID}", request.CaseID);
            throw new NotFoundException($"Case ID {request.CaseID} not found");
        }

        // 2. Verify that the new status exists
        var newStatus = await _context.CaseStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.StatusID == request.NewStatusID, cancellationToken);

        if (newStatus == null)
        {
            _logger.LogWarning("Status not found: {StatusID}", request.NewStatusID);
            throw new NotFoundException($"Status ID {request.NewStatusID} not found");
        }

        // 3. Check whether it is the same status and skip if so
        if (caseToUpdate.StatusID == request.NewStatusID)
        {
            _logger.LogWarning("Attempting to set the same status: {CaseID}", request.CaseID);
            throw new ValidationException(new List<string>
            {
                "The new status is the same as the current status"
            });
        }

        // 4. Store old status
        int oldStatusID = caseToUpdate.StatusID;

        // 5. Update case status
        // FIX: sync IsClosed / ClosureDate with the new status.
        // Previously the Case.IsClosed flag (used by vw_ActiveCases /
        // vw_ClosedCases) was never updated here, so a case could
        // move to a "Closed" status yet still show up as active.
        caseToUpdate.StatusID = request.NewStatusID;
        caseToUpdate.IsClosed = newStatus.IsClosed;

        if (newStatus.IsClosed)
        {
            // Only stamp ClosureDate the first time it becomes closed
            caseToUpdate.ClosureDate ??= DateTime.UtcNow.Date;
        }
        else
        {
            // Case re-opened (moved to a non-closed status) - clear closure date
            caseToUpdate.ClosureDate = null;
        }

        caseToUpdate.ModifiedBy = currentUserId;
        caseToUpdate.ModifiedDate = DateTime.UtcNow;

        // 6. Create status history entry
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

        // 7. Create Audit Log
        var auditLog = _auditService.Create(
            currentUserId,
            $"Case Status Update: {caseToUpdate.CaseNumber} - {newStatus.StatusName}");

        _context.AuditLogs.Add(auditLog);

        // 8. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Case status successfully updated: {CaseID} from {OldStatus} to {NewStatus}",
            request.CaseID, oldStatusID, request.NewStatusID);

        return true;
    }

    /// <summary>
    /// Get current logged-in user ID from HttpContext.
    /// SECURITY FIX: see UpdateCaseHandler.GetCurrentUserId for full
    /// rationale - previously defaulted to UserID = 1 (SuperAdmin) instead
    /// of failing when the identity claim was missing.
    /// </summary>
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