using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Security;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.CaseAssignments.Commands.AssignCase
{
    /// <summary>
    /// SRS Reference: Litigation_Tracking_System_Case_SRS.docx UC-04 "Assign Case to Counsel"
    /// FR-06 "System shall allow assignment of cases to legal officers and counsel"
    /// Also triggers a Notification (SRS: "System sends notification")
    /// </summary>
    public class AssignCaseHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,IHttpContextAccessor _httpContextAccessor,ILogger<AssignCaseHandler> _logger) : IRequestHandler<AssignCaseCommand, long>
    {
        // =====================================================
        // HANDLE — assigns a lawyer/counsel to a case (SRS UC-04)
        // Validates the case and the user-being-assigned both belong to
        // the caller's own firm, blocks a duplicate active assignment of
        // the same user+type on the same case, creates the assignment,
        // syncs Case.CurrentLegalOfficerID if this is the lead counsel,
        // sends the assigned user a notification, and writes an audit
        // log entry.
        // =====================================================
        public async Task<long> Handle(AssignCaseCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await _context.Cases.FirstOrDefaultAsync(c => c.CaseID == request.Assignment.CaseID, cancellationToken);
            if (caseEntity == null || (caseEntity.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Case ID {request.Assignment.CaseID} not found");

            // FIX: user being assigned must belong to the same firm as the case
            // (otherwise a lawyer from another firm could be "assigned" cross-tenant)
            var userExists = await _context.Users.AnyAsync(u =>
                u.UserID == request.Assignment.UserID &&
                u.IsActive &&
                u.FirmID == caseEntity.FirmID, cancellationToken);
            if (!userExists)
                throw new NotFoundException($"User ID {request.Assignment.UserID} not found, is inactive, or does not belong to this firm");

            // Prevent duplicate active assignment of the same user+type on the same case
            var duplicate = await _context.CaseAssignments.AnyAsync(a =>
                a.CaseID == request.Assignment.CaseID &&
                a.UserID == request.Assignment.UserID &&
                a.AssignmentType == request.Assignment.AssignmentType &&
                a.EndDate == null, cancellationToken);

            if (duplicate)
                throw new ValidationException(new List<string> { "This user is already actively assigned to this case with this type" });

            int currentUserId = GetCurrentUserId();

            var assignment = new CaseAssignment
            {
                CaseID = request.Assignment.CaseID,
                UserID = request.Assignment.UserID,
                AssignmentType = request.Assignment.AssignmentType,
                LeadCounsel = request.Assignment.LeadCounsel,
                AssignedBy = currentUserId,
                AssignedDate = DateTime.UtcNow,
                Remarks = request.Assignment.Remarks
            };

            _context.CaseAssignments.Add(assignment);

            // If lead counsel, sync Case.CurrentLegalOfficerID for quick lookups / dashboards
            if (request.Assignment.LeadCounsel)
            {
                caseEntity.CurrentLegalOfficerID = request.Assignment.UserID;
                caseEntity.ModifiedBy = currentUserId;
                caseEntity.ModifiedDate = DateTime.UtcNow;
            }

            // SRS: "System sends notification" (UC-04 postcondition)
            _context.Notifications.Add(new Notification
            {
                NotificationTypeID = 3, // CaseAssignment (seeded in AppDbContext)
                UserID = request.Assignment.UserID,
                CaseID = request.Assignment.CaseID,
                Subject = "New case assigned",
                Message = $"You have been assigned to case {caseEntity.CaseNumber} ({caseEntity.CaseTitle}) as '{request.Assignment.AssignmentType}'.",
                Priority = "Medium",
                CreatedDate = DateTime.UtcNow
            });

            _context.AuditLogs.Add(_auditService.Create(currentUserId,$"Case Assigned: UserID {request.Assignment.UserID} to Case {request.Assignment.CaseID} as {request.Assignment.AssignmentType}"));

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Case {CaseID} assigned to User {UserID}", request.Assignment.CaseID, request.Assignment.UserID);

            return assignment.AssignmentID;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}