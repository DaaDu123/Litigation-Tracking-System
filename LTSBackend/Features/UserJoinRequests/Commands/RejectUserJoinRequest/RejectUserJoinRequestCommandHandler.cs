using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Commands.RejectUserJoinRequest;

public class RejectUserJoinRequestCommandHandler(AppDbContext _context, IEmailService _emailService, IAuditService _auditService,
    ILogger<RejectUserJoinRequestCommandHandler> _logger) : IRequestHandler<RejectUserJoinRequestCommand, bool>
{
    // =====================================================
    // HANDLE — declines a pending join request
    // Same tenant-scoping note as ApproveUserJoinRequestCommandHandler:
    // the query filter already prevents a FirmAdmin from reaching a
    // request aimed at a different firm. Refuses if the request isn't
    // still Pending. Marks it Rejected with an optional reason, writes
    // an audit log entry, and emails the requester (best-effort).
    // =====================================================
    public async Task<bool> Handle(RejectUserJoinRequestCommand request, CancellationToken cancellationToken)
    {
        var joinRequest = await _context.UserJoinRequests.FirstOrDefaultAsync(x => x.RequestID == request.RequestID, cancellationToken);

        if (joinRequest == null)
            throw new NotFoundException("Join request not found.");

        if (joinRequest.Status != "Pending")
            throw new ValidationException([$"This request has already been {joinRequest.Status.ToLower()}."]);

        joinRequest.Status = "Rejected";
        joinRequest.ReviewedBy = request.ActingUserID;
        joinRequest.ReviewedAt = DateTime.UtcNow;
        joinRequest.RejectionReason = request.Reason;

        var auditLog = _auditService.Create(request.ActingUserID, $"Rejected join request #{joinRequest.RequestID} for {joinRequest.Email} (firm {joinRequest.FirmID})");
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User join request {RequestId} rejected by {ActingUserId}", joinRequest.RequestID, request.ActingUserID);

        // Best-effort - don't fail the rejection itself if the email send fails.
        // The user's own account (if UserID-linked) still exists and can
        // request a different firm right away - rejection never deletes it.
        if (!string.IsNullOrWhiteSpace(joinRequest.Email))
        {
            try
            {
                var reasonText = string.IsNullOrWhiteSpace(request.Reason) ? string.Empty : $" Reason: {request.Reason}";
                await _emailService.SendNotificationEmailAsync(
                    joinRequest.Email,
                    joinRequest.FullName ?? joinRequest.Email,
                    "Your Request to Join the Firm Was Not Approved",
                    $"Your request to join the firm was not approved.{reasonText} You're welcome to request a different firm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rejection email to {Email}", joinRequest.Email);
            }
        }

        return true;
    }
}
