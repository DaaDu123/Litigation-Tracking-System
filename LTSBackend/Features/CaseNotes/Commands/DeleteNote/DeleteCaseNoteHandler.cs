using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.CaseNotes.Commands.DeleteNote
{
    public class DeleteCaseNoteHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,IHttpContextAccessor _httpContextAccessor) : IRequestHandler<DeleteCaseNoteCommand, bool>
    {
        // =====================================================
        // HANDLE — permanently removes a case note
        // Same rule as Update: firm isolation, plus only the note's own
        // author or a senior role (SuperAdmin/FirmAdmin/Partner) may
        // delete it.
        // =====================================================
        public async Task<bool> Handle(DeleteCaseNoteCommand request, CancellationToken cancellationToken)
        {
            var note = await _context.CaseNotes.Include(n => n.Case).FirstOrDefaultAsync(n => n.NoteID == request.NoteID, cancellationToken);

            if (note == null || (note.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Note ID {request.NoteID} not found");

            int currentUserId = GetCurrentUserId();
            var currentUserRoleId = await _context.Users.Where(u => u.UserID == currentUserId).Select(u => u.RoleID).FirstOrDefaultAsync(cancellationToken);
            bool isElevated = currentUserRoleId is 1 or 2 or 3;

            if (note.UserID != currentUserId && !isElevated)
                throw new UnauthorizedException("Only the note's author or senior staff can delete it");

            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Case Note Deleted: NoteID {note.NoteID}"));

            _context.CaseNotes.Remove(note);
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