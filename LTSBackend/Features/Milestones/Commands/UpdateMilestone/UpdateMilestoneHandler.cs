using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Milestones.Commands.UpdateMilestone
{
    public class UpdateMilestoneHandler(AppDbContext _context, IAuditService _auditService,
        ICurrentUserService _currentUser, IPermissionService _permissionService, IHttpContextAccessor _httpContextAccessor) : IRequestHandler<UpdateMilestoneCommand, bool>
    {
        // =====================================================
        // HANDLE — edits an existing milestone
        // Same visibility rule as Create/Complete.
        // =====================================================
        public async Task<bool> Handle(UpdateMilestoneCommand request, CancellationToken cancellationToken)
        {
            var milestone = await _context.CaseMilestones
                .Include(m => m.Case)
                .FirstOrDefaultAsync(m => m.MilestoneID == request.Milestone.MilestoneID, cancellationToken);

            if (milestone == null || (milestone.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Milestone ID {request.Milestone.MilestoneID} not found");

            // SECURITY FIX (IDOR): see CreateMilestoneHandler for rationale.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, milestone.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Milestone ID {request.Milestone.MilestoneID} not found");
                }
            }

            milestone.Milestone = request.Milestone.Milestone;
            milestone.MilestoneDate = request.Milestone.MilestoneDate;
            milestone.Description = request.Milestone.Description;

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Milestone Updated: MilestoneID {milestone.MilestoneID}"));

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