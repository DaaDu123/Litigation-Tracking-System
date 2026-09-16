using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.CaseAssignments.Commands.UpdateAssignment
{
    public class UpdateAssignmentHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,IHttpContextAccessor _httpContextAccessor) : IRequestHandler<UpdateAssignmentCommand, bool>
    {
        // =====================================================
        // HANDLE — edits an existing, still-active case assignment
        // Verifies the assignment belongs to the caller's own firm and
        // hasn't already ended, then updates its type/lead-counsel
        // flag/remarks and writes an audit log entry.
        // =====================================================
        public async Task<bool> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
        {
            var assignment = await _context.CaseAssignments.Include(a => a.Case).FirstOrDefaultAsync(a => a.AssignmentID == request.Assignment.AssignmentID, cancellationToken);

            if (assignment == null || (assignment.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Assignment ID {request.Assignment.AssignmentID} not found");

            if (assignment.EndDate != null)
                throw new ValidationException(new List<string> { "An ended assignment cannot be updated" });

            assignment.AssignmentType = request.Assignment.AssignmentType;
            assignment.LeadCounsel = request.Assignment.LeadCounsel;
            assignment.Remarks = request.Assignment.Remarks;

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Assignment Updated: AssignmentID {assignment.AssignmentID}"));

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