using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Milestones.Commands.CreateMilestone
{
    public class CreateMilestoneHandler(AppDbContext _context, IAuditService _auditService, ICurrentUserService _currentUser, IPermissionService _permissionService,
      IHttpContextAccessor _httpContextAccessor) : IRequestHandler<CreateMilestoneCommand, long>
    {
        // =====================================================
        // HANDLE — adds a new milestone against a case
        // Verifies the case belongs to the caller's own firm, and — since
        // Create is open to AssociateLawyer/Moharrir at the controller —
        // that the caller has full case-directory visibility or is
        // actually assigned to this case.
        // =====================================================
        public async Task<long> Handle(CreateMilestoneCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await _context.Cases.FirstOrDefaultAsync(c => c.CaseID == request.Milestone.CaseID, cancellationToken);
            if (caseEntity == null || (caseEntity.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Case ID {request.Milestone.CaseID} not found");

            // SECURITY FIX (IDOR): controller allows RoleNames.AllLawyers
            // (includes AssociateLawyer/Moharrir), who must be scoped to
            // their assigned cases only.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.Milestone.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Case ID {request.Milestone.CaseID} not found");
                }
            }

            var milestone = new CaseMilestone
            {
                CaseID = request.Milestone.CaseID,
                Milestone = request.Milestone.Milestone,
                MilestoneDate = request.Milestone.MilestoneDate,
                Description = request.Milestone.Description
            };

            _context.CaseMilestones.Add(milestone);

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Milestone Created: {milestone.Milestone} for Case {request.Milestone.CaseID}"));

            await _context.SaveChangesAsync(cancellationToken);
            return milestone.MilestoneID;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}