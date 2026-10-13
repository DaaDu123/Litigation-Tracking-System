using LTSBackend.Data;
using LTSBackend.Features.CaseNotes.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseNotes.Queries.GetCaseNotes
{




    public class GetCaseNotesHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService, IHttpContextAccessor _httpContextAccessor)
        : IRequestHandler<GetCaseNotesQuery, List<CaseNoteDetailDTO>>
    {








        public async Task<List<CaseNoteDetailDTO>> Handle(GetCaseNotesQuery request, CancellationToken cancellationToken)
        {
            int currentUserId = GetCurrentUserId();







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
            bool isElevated = currentUserRoleId is 1 or 2 or 3 or 4;

            var query = _context.CaseNotes
                .AsNoTracking()
                .Include(n => n.User)
                .Include(n => n.Case)
                .Where(n => n.CaseID == request.CaseID);


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