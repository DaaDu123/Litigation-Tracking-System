using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Deadlines.Commands.UpdateDeadline
{
    public class UpdateDeadlineHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,IPermissionService _permissionService,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<UpdateDeadlineCommand, bool>
    {
        // =====================================================
        // HANDLE — edits an existing, not-yet-completed deadline
        // Firm isolation plus the same assignment check as
        // CreateDeadlineHandler. Refuses to edit a deadline that's
        // already marked complete.
        // =====================================================
        public async Task<bool> Handle(UpdateDeadlineCommand request, CancellationToken cancellationToken)
        {
            var deadline = await _context.Deadlines.Include(d => d.Case).FirstOrDefaultAsync(d => d.DeadlineID == request.Deadline.DeadlineID, cancellationToken);

            if (deadline == null || (deadline.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Deadline ID {request.Deadline.DeadlineID} not found");

            // SECURITY FIX (IDOR): see CreateDeadlineHandler for rationale.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, deadline.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Deadline ID {request.Deadline.DeadlineID} not found");
                }
            }

            if (deadline.Completed)
                throw new ValidationException(new List<string> { "A completed deadline cannot be updated" });

            deadline.DeadlineType = request.Deadline.DeadlineType;
            deadline.DueDate = request.Deadline.DueDate;
            deadline.ReminderDays = request.Deadline.ReminderDays;
            deadline.Remarks = request.Deadline.Remarks;

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Deadline Updated: DeadlineID {deadline.DeadlineID}"));

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}