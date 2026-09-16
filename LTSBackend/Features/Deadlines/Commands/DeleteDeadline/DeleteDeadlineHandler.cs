using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Deadlines.Commands.DeleteDeadline
{
    public class DeleteDeadlineHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<DeleteDeadlineCommand, bool>
    {
        // =====================================================
        // HANDLE — permanently removes a deadline
        // Firm-scoped lookup before deleting; writes an audit log entry.
        // =====================================================
        public async Task<bool> Handle(DeleteDeadlineCommand request, CancellationToken cancellationToken)
        {
            var deadline = await _context.Deadlines.Include(d => d.Case).FirstOrDefaultAsync(d => d.DeadlineID == request.DeadlineID, cancellationToken);

            if (deadline == null || (deadline.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Deadline ID {request.DeadlineID} not found");

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Deadline Deleted: DeadlineID {deadline.DeadlineID}"));

            _context.Deadlines.Remove(deadline);
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