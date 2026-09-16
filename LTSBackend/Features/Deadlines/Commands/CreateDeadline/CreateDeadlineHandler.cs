using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Deadlines.Commands.CreateDeadline
{
    public class CreateDeadlineHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,IPermissionService _permissionService,
        IHttpContextAccessor _httpContextAccessor,ILogger<CreateDeadlineHandler> _logger) : IRequestHandler<CreateDeadlineCommand, long>
    {
        // =====================================================
        // HANDLE — adds a new deadline against a case
        // Verifies the case belongs to the caller's own firm, and —
        // since Create is open to AssociateLawyer/Moharrir at the
        // controller — that the caller has full case-directory
        // visibility or is actually assigned to this case. Saves the
        // deadline with the caller's own chosen ReminderDays (see
        // ReminderService for how that's used).
        // =====================================================
        public async Task<long> Handle(CreateDeadlineCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await _context.Cases.FirstOrDefaultAsync(c => c.CaseID == request.Deadline.CaseID, cancellationToken);
            if (caseEntity == null || (caseEntity.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Case ID {request.Deadline.CaseID} not found");

            // SECURITY FIX (IDOR): controller allows RoleNames.AllLawyers
            // (includes AssociateLawyer/Moharrir), who must be scoped to
            // their assigned cases only.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.Deadline.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Case ID {request.Deadline.CaseID} not found");
                }
            }

            var deadline = new Deadline
            {
                CaseID = request.Deadline.CaseID,
                DeadlineType = request.Deadline.DeadlineType,
                DueDate = request.Deadline.DueDate,
                ReminderDays = request.Deadline.ReminderDays,
                Remarks = request.Deadline.Remarks,
                Completed = false
            };

            _context.Deadlines.Add(deadline);

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Deadline Created: {deadline.DeadlineType} due {deadline.DueDate:d} for Case {request.Deadline.CaseID}"));

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deadline created: {DeadlineID} for Case {CaseID}", deadline.DeadlineID, request.Deadline.CaseID);

            return deadline.DeadlineID;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}