using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Deadlines.Commands.CompleteDeadline
{
    public class CompleteDeadlineHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,
        IPermissionService _permissionService,IHttpContextAccessor _httpContextAccessor) : IRequestHandler<CompleteDeadlineCommand, bool>
    {
        // =====================================================
        // HANDLE — marks a deadline complete
        // Same visibility rule as Create/Update. Sets Completed = true
        // and stamps CompletedDate, which stops it from appearing in
        // "upcoming" lists and stops ReminderService from emailing
        // about it.
        // =====================================================
        public async Task<bool> Handle(CompleteDeadlineCommand request, CancellationToken cancellationToken)
        {
            var deadline = await _context.Deadlines.Include(d => d.Case).FirstOrDefaultAsync(d => d.DeadlineID == request.DeadlineID, cancellationToken);

            if (deadline == null || (deadline.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Deadline ID {request.DeadlineID} not found");

            // SECURITY FIX (IDOR): see CreateDeadlineHandler for rationale.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, deadline.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Deadline ID {request.DeadlineID} not found");
                }
            }

            deadline.Completed = true;
            deadline.CompletedDate = DateTime.UtcNow;

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Deadline Completed: DeadlineID {deadline.DeadlineID}"));

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