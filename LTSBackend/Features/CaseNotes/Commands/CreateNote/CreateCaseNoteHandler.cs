using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.CaseNotes.Commands.CreateNote
{
    public class CreateCaseNoteHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,
        IPermissionService _permissionService,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<CreateCaseNoteCommand, long>
    {
        // =====================================================
        // HANDLE — adds a new note against a case
        // Verifies the case belongs to the caller's own firm, and — since
        // the controller allows AssociateLawyer/Moharrir/InternParalegal
        // too — that the caller either has full case-directory visibility
        // or is actually assigned to this specific case, before recording
        // the note.
        // =====================================================
        public async Task<long> Handle(CreateCaseNoteCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await _context.Cases.FirstOrDefaultAsync(c => c.CaseID == request.Note.CaseID, cancellationToken);
            if (caseEntity == null || (caseEntity.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Case ID {request.Note.CaseID} not found");

            // SECURITY FIX (IDOR): controller allows RoleNames.AllFirmUsers
            // (includes AssociateLawyer/Moharrir/InternParalegal), who per the
            // roles spec must be scoped to their assigned cases only.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.Note.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Case ID {request.Note.CaseID} not found");
                }
            }

            int currentUserId = GetCurrentUserId();

            var note = new CaseNote
            {
                CaseID = request.Note.CaseID,
                UserID = currentUserId,
                NoteType = request.Note.NoteType,
                Notes = request.Note.Notes,
                CreatedDate = DateTime.UtcNow
            };

            _context.CaseNotes.Add(note);
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Case Note Added ({note.NoteType}) for Case {request.Note.CaseID}"));

            await _context.SaveChangesAsync(cancellationToken);
            return note.NoteID;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}