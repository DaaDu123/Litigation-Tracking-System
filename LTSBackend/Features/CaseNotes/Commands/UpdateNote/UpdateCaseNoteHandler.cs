using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.CaseNotes.Commands.UpdateNote
{
    /// <summary>
    /// SRS: Only the note author or Partner/FirmAdmin/SuperAdmin may edit a note
    /// (protects "Confidential" internal notes from being altered by unrelated users)
    /// </summary>
    public class UpdateCaseNoteHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<UpdateCaseNoteCommand, bool>
    {
        // =====================================================
        // HANDLE — edits an existing case note
        // Enforces firm isolation, then only the note's own author or a
        // senior role (SuperAdmin/FirmAdmin/Partner) may edit it — protects
        // confidential internal notes from being altered by unrelated staff.
        // =====================================================
        public async Task<bool> Handle(UpdateCaseNoteCommand request, CancellationToken cancellationToken)
        {
            var note = await _context.CaseNotes.Include(n => n.Case).FirstOrDefaultAsync(n => n.NoteID == request.Note.NoteID, cancellationToken);

            if (note == null || (note.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Note ID {request.Note.NoteID} not found");

            int currentUserId = GetCurrentUserId();
            var currentUserRoleId = await GetCurrentUserRoleId(currentUserId, cancellationToken);
            bool isElevated = currentUserRoleId is 1 or 2 or 3; // SuperAdmin, FirmAdmin, Partner

            if (note.UserID != currentUserId && !isElevated)
                throw new UnauthorizedException("Only the note's author or senior staff can edit it");

            note.NoteType = request.Note.NoteType;
            note.Notes = request.Note.Notes;

            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Case Note Updated: NoteID {note.NoteID}"));

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private async Task<int?> GetCurrentUserRoleId(int userId, CancellationToken ct)
        {
            return await _context.Users.Where(u => u.UserID == userId).Select(u => u.RoleID).FirstOrDefaultAsync(ct);
        }
    }
}