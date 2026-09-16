using LTSBackend.Data;
using LTSBackend.Features.CaseNotes.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseNotes.Queries.GetCaseNotes
{
    /// <summary>
    /// SRS: "Confidential" notes are excluded for users below Associate/Moharrir level
    /// unless they are the note's author, to protect internal legal opinions.
    /// </summary>
    public class GetCaseNotesHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService, IHttpContextAccessor _httpContextAccessor)
        : IRequestHandler<GetCaseNotesQuery, List<CaseNoteDetailDTO>>
    {
        // =====================================================
        // HANDLE — lists a case's notes, filtered for confidentiality and assignment
        // Enforces the same "must be assigned or have full visibility"
        // check as CreateCaseNoteHandler (returns an empty list rather than
        // an error, so the case's existence isn't disclosed), firm
        // isolation, and hides "Confidential"-type notes from anyone who
        // isn't the author or Associate-Lawyer-and-above.
        // =====================================================
        public async Task<List<CaseNoteDetailDTO>> Handle(GetCaseNotesQuery request, CancellationToken cancellationToken)
        {
            int currentUserId = GetCurrentUserId();

            // SECURITY FIX (IDOR / broken access control): previously only firm
            // (tenant) scoping was applied, so any firm user - including
            // AssociateLawyer, Moharrir, InternParalegal - could read case
            // notes (including confidential legal opinions, gated separately
            // below) for a case they are not assigned to. Mirrors the check
            // already used in GetCaseAssignmentsHandler.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        return new List<CaseNoteDetailDTO>();
                }
            }

            var currentUserRoleId = await _context.Users.Where(u => u.UserID == currentUserId).Select(u => u.RoleID).FirstOrDefaultAsync(cancellationToken);
            bool isElevated = currentUserRoleId is 1 or 2 or 3 or 4; // SuperAdmin..AssociateLawyer can see confidential notes

            var query = _context.CaseNotes
                .AsNoTracking()
                .Include(n => n.User)
                .Include(n => n.Case)
                .Where(n => n.CaseID == request.CaseID);

            // Multi-tenant isolation - don't leak another firm's case notes
                query = query.Where(n => n.Case.FirmID == _currentUser.FirmID);

            var notes = await query
                .Where(n => n.NoteType != "Confidential" || isElevated || n.UserID == currentUserId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync(cancellationToken);

            return notes.Select(n => new CaseNoteDetailDTO
            {
                NoteID = n.NoteID,
                CaseID = n.CaseID,
                UserID = n.UserID,
                UserName = n.User?.FullName,
                NoteType = n.NoteType,
                Notes = n.Notes,
                CreatedDate = n.CreatedDate
            }).ToList();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}