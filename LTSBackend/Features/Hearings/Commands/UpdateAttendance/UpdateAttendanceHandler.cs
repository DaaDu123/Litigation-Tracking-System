using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Hearings.Commands.UpdateAttendance
{
    public class UpdateAttendanceHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,
        IPermissionService _permissionService,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<UpdateAttendanceCommand, bool>
    {
        // =====================================================
        // HANDLE — edits an existing attendance record
        // Same visibility rule as RecordAttendanceHandler.
        // =====================================================
        public async Task<bool> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendance = await _context.HearingAttendances
                .Include(a => a.Hearing).ThenInclude(h => h.Case)
                .FirstOrDefaultAsync(a => a.AttendanceID == request.AttendanceId, cancellationToken);

            if (attendance == null || (attendance.Hearing.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Attendance ID {request.AttendanceId} not found");

            // SECURITY FIX (IDOR): see RecordAttendanceHandler for rationale.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, attendance.Hearing.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Attendance ID {request.AttendanceId} not found");
                }
            }

            attendance.Present = request.IsPresent;
            attendance.AttendanceRole = request.AttendanceRole;
            attendance.ArrivalTime = request.ArrivalTime;
            attendance.DepartureTime = request.DepartureTime;
            attendance.Remarks = request.Remarks;

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Hearing Attendance Updated: AttendanceID {attendance.AttendanceID}"));

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