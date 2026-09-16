using LTSBackend.Data;
using LTSBackend.Features.Hearings.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Hearings.Queries.GetHearingAttendance
{
    public class GetHearingAttendanceHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService) : IRequestHandler<GetHearingAttendanceQuery, List<HearingAttendanceDTO>>
    {
        // =====================================================
        // HANDLE — lists a hearing's attendance records, scoped by assignment
        // Same assignment-or-full-visibility rule as
        // RecordAttendanceHandler (empty list, not an error, if denied),
        // plus firm isolation.
        // =====================================================
        public async Task<List<HearingAttendanceDTO>> Handle(GetHearingAttendanceQuery request, CancellationToken cancellationToken)
        {
            // SECURITY FIX (IDOR): see RecordAttendanceHandler for rationale.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    var caseId = await _context.Hearings.Where(h => h.HearingID == request.HearingId).Select(h => (long?)h.CaseID).FirstOrDefaultAsync(cancellationToken);
                    if (caseId == null || !await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, caseId.Value, cancellationToken))
                        return new List<HearingAttendanceDTO>();
                }
            }

            var query = _context.HearingAttendances
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Hearing)
                .Where(a => a.HearingID == request.HearingId);

                query = query.Where(a => a.Hearing.Case.FirmID == _currentUser.FirmID);

            return await query.Select(a => new HearingAttendanceDTO
                {
                    AttendanceId = a.AttendanceID,
                    HearingId = a.HearingID,
                    UserId = a.UserID,
                    UserName = a.User.FullName,
                    AttendanceRole = a.AttendanceRole,
                    IsPresent = a.Present,
                    ArrivalTime = a.ArrivalTime,
                    DepartureTime = a.DepartureTime,
                    Remarks = a.Remarks
                })
                .ToListAsync(cancellationToken);
        }
    }
}