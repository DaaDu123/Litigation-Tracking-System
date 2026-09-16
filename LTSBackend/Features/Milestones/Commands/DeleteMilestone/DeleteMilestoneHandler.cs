using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Milestones.Commands.DeleteMilestone
{
    public class DeleteMilestoneHandler(AppDbContext _context, IAuditService _auditService,
        ICurrentUserService _currentUser, IHttpContextAccessor _httpContextAccessor) : IRequestHandler<DeleteMilestoneCommand, bool>
    {
        // =====================================================
        // HANDLE — permanently removes a milestone
        // Firm-scoped lookup before deleting; writes an audit log entry.
        // =====================================================
        public async Task<bool> Handle(DeleteMilestoneCommand request, CancellationToken cancellationToken)
        {
            var milestone = await _context.CaseMilestones
                .Include(m => m.Case)
                .FirstOrDefaultAsync(m => m.MilestoneID == request.MilestoneID, cancellationToken);

            if (milestone == null || (milestone.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Milestone ID {request.MilestoneID} not found");

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Milestone Deleted: MilestoneID {milestone.MilestoneID}"));

            _context.CaseMilestones.Remove(milestone);
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