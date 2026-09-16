using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Milestones.Commands.CompleteMilestone
{
    public class CompleteMilestoneHandler(
        AppDbContext _context,
        IAuditService _auditService,
        ICurrentUserService _currentUser,
        IPermissionService _permissionService,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<CompleteMilestoneCommand, bool>
    {
        // =====================================================
        // HANDLE — marks a milestone complete
        // Same visibility rule as Create. Records who completed it and
        // when.
        // =====================================================
        public async Task<bool> Handle(CompleteMilestoneCommand request, CancellationToken cancellationToken)
        {
            var milestone = await _context.CaseMilestones
                .Include(m => m.Case)
                .FirstOrDefaultAsync(m => m.MilestoneID == request.MilestoneID, cancellationToken);

            if (milestone == null || (milestone.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Milestone ID {request.MilestoneID} not found");

            // SECURITY FIX (IDOR): see CreateMilestoneHandler for rationale.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, milestone.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Milestone ID {request.MilestoneID} not found");
                }
            }

            int currentUserId = GetCurrentUserId();

            milestone.CompletedBy = currentUserId;
            milestone.CompletedDate = DateTime.UtcNow;

            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Milestone Completed: MilestoneID {milestone.MilestoneID}"));

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