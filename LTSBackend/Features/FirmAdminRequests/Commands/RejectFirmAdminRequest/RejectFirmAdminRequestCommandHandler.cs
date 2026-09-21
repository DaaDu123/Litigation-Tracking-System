using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.FirmAdminRequests.Commands.RejectFirmAdminRequest;

public class RejectFirmAdminRequestCommandHandler(AppDbContext _context,IEmailService _emailService,IAuditService _auditService,
    ILogger<RejectFirmAdminRequestCommandHandler> _logger) : IRequestHandler<RejectFirmAdminRequestCommand, bool>
{
    // =====================================================
    // HANDLE — declines a pending Firm Admin request
    // Refuses if the request isn't still Pending. Marks it Rejected with
    // an optional reason, writes an audit log entry, and emails the
    // requester (best-effort).
    // =====================================================
    public async Task<bool> Handle(RejectFirmAdminRequestCommand request, CancellationToken cancellationToken)
    {
        var firmAdminRequest = await _context.FirmAdminRequests.FirstOrDefaultAsync(x => x.RequestID == request.RequestID, cancellationToken);

        if (firmAdminRequest == null)
            throw new NotFoundException("Firm Admin request not found.");

        if (firmAdminRequest.Status != "Pending")
            throw new ValidationException([$"This request has already been {firmAdminRequest.Status.ToLower()}."]);

        firmAdminRequest.Status = "Rejected";
        firmAdminRequest.ReviewedBy = request.ActingUserID;
        firmAdminRequest.ReviewedAt = DateTime.UtcNow;
        firmAdminRequest.RejectionReason = request.Reason;

        var auditLog = _auditService.Create(request.ActingUserID,$"Rejected Firm Admin request #{firmAdminRequest.RequestID} for {firmAdminRequest.AdminEmail}");
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Firm Admin request {RequestId} rejected by {ActingUserId}", firmAdminRequest.RequestID, request.ActingUserID);

        // Best-effort - don't fail the rejection itself if the email send fails.
        try
        {
            var reasonText = string.IsNullOrWhiteSpace(request.Reason) ? string.Empty : $" Reason: {request.Reason}";
            await _emailService.SendNotificationEmailAsync(
                firmAdminRequest.AdminEmail,
                firmAdminRequest.AdminFullName ?? firmAdminRequest.AdminEmail,
                "Your Firm Admin Request Was Not Approved",
                $"Your request to create a firm workspace was not approved.{reasonText} " +
                $"You're welcome to submit a new request.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send rejection email to {Email}", firmAdminRequest.AdminEmail);
        }

        return true;
    }
}
