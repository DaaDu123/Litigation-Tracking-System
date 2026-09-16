using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Hearings.Commands.RecordAttendance
{
    /// <summary>
    /// SRS Reference: Complete Database Schema - HearingAttendance table
    /// (attendance tracking for lawyers/officers present at a hearing)
    /// </summary>
    public class RecordAttendanceHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,
        IPermissionService _permissionService,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<RecordAttendanceCommand, long>
    {
        // =====================================================
        // HANDLE — logs a user's attendance at a hearing
        // Firm isolation plus the assignment-or-full-visibility check
        // (Record is open to every firm role at the controller). Blocks
        // recording the same user's attendance twice for the same
        // hearing.
        // =====================================================
        public async Task<long> Handle(RecordAttendanceCommand request, CancellationToken cancellationToken)
        {
            var hearing = await _context.Hearings
                .Include(h => h.Case)
                .FirstOrDefaultAsync(h => h.HearingID == request.Attendance.HearingId, cancellationToken);

            if (hearing == null || (hearing.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Hearing ID {request.Attendance.HearingId} not found");

            // SECURITY FIX (IDOR): controller allows RoleNames.AllFirmUsers
            // (includes AssociateLawyer/Moharrir/InternParalegal), who must be
            // scoped to their assigned cases only.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, hearing.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Hearing ID {request.Attendance.HearingId} not found");
                }
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserID == request.Attendance.UserId, cancellationToken);
            if (!userExists)
                throw new NotFoundException($"User ID {request.Attendance.UserId} not found");

            var duplicate = await _context.HearingAttendances.AnyAsync(a =>
                a.HearingID == request.Attendance.HearingId && a.UserID == request.Attendance.UserId, cancellationToken);
            if (duplicate)
                throw new ValidationException(new List<string> { "This user's attendance for this hearing has already been recorded" });

            var attendance = new HearingAttendance
            {
                HearingID = request.Attendance.HearingId,
                UserID = request.Attendance.UserId,
                Present = request.Attendance.IsPresent,
                Remarks = request.Attendance.Remarks
            };

            _context.HearingAttendances.Add(attendance);

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId,
                $"Hearing Attendance Recorded: UserID {request.Attendance.UserId} for HearingID {request.Attendance.HearingId} - Present: {request.Attendance.IsPresent}"));

            await _context.SaveChangesAsync(cancellationToken);
            return attendance.AttendanceID;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}