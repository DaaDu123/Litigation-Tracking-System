using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Security;
using LTSBackend.Services;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequest;

public class SubmitFirmAdminRequestCommandHandler(AppDbContext _context,IPasswordService _passwordService,IEmailService _emailService,
    ILogger<SubmitFirmAdminRequestCommandHandler> _logger) : IRequestHandler<SubmitFirmAdminRequestCommand, int>
{
    // NotificationTypeID = 6 ("FirmAdminRequest") - seeded in AppDbContext.SeedNotificationTypes.
    private const int FirmAdminRequestNotificationTypeId = 6;

    // =====================================================
    // HANDLE — anonymous self-service request for a new firm workspace.
    // Firm Admin registration is deliberately reduced to Email + Password
    // only - no FirmCode, no firm name, no personal details. The Firm and
    // the requester's profile are both filled in with system-generated
    // placeholders on SuperAdmin approval and then completed for real by
    // the Firm Admin during the mandatory post-login profile-completion
    // step (CompleteFirmAdminProfileCommand). Checks the admin email
    // isn't already a live user or tied to another still-pending request,
    // hashes the password immediately (plaintext is never stored), saves
    // the request as Pending, and alerts every active SuperAdmin.
    // =====================================================
    public async Task<int> Handle(SubmitFirmAdminRequestCommand request, CancellationToken cancellationToken)
    {
        var adminEmail = request.Email.Trim();

        // Admin email must not already exist as a real user, and must
        // not already be tied to another still-Pending request (either
        // kind - Firm Admin or Firm User).
        bool emailTaken = await _context.Users.AsNoTracking().AnyAsync(x => x.Email == adminEmail && !x.IsDeleted, cancellationToken);

        if (emailTaken)
            throw new ValidationException([$"Email '{adminEmail}' already exists."]);

        bool emailPending = await _context.FirmAdminRequests.AsNoTracking().AnyAsync(x => x.AdminEmail == adminEmail && x.Status == "Pending", cancellationToken);

        if (emailPending)
            throw new ValidationException([$"A request for email '{adminEmail}' is already pending Super Admin review."]);

        bool joinPending = await _context.UserJoinRequests.AsNoTracking().AnyAsync(x => x.Email == adminEmail && x.Status == "Pending", cancellationToken);

        if (joinPending)
            throw new ValidationException([$"A request for email '{adminEmail}' is already pending review."]);

        // Persist the pending request. Password is hashed now - the
        // plaintext is never stored - so Approve just copies the hash
        // onto the new User row. Firm/admin detail fields are left null;
        // ApproveFirmAdminRequestCommandHandler fills placeholders.
        var firmAdminRequest = new FirmAdminRequest
        {
            AdminEmail = adminEmail,
            AdminPasswordHash = _passwordService.HashPassword(request.Password),
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _context.FirmAdminRequests.Add(firmAdminRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Firm Admin request {RequestId} submitted by {AdminEmail}", firmAdminRequest.RequestID, adminEmail);

        await NotifySuperAdminsAsync(firmAdminRequest, cancellationToken);

        return firmAdminRequest.RequestID;
    }

    private async Task NotifySuperAdminsAsync(FirmAdminRequest firmAdminRequest, CancellationToken cancellationToken)
    {
        var superAdmins = await _context.Users.AsNoTracking().Where(x => x.RoleID == (int)UserRole.SuperAdmin && x.IsActive && !x.IsDeleted).ToListAsync(cancellationToken);

        if (superAdmins.Count == 0)
        {
            _logger.LogWarning("No active Super Admin found to notify about Firm Admin request {RequestId}", firmAdminRequest.RequestID);
            return;
        }

        var subject = "New Firm Admin Request";
        var message = $"{firmAdminRequest.AdminEmail} has requested to create a new firm workspace and become its Firm Admin. " +
                       $"Please review and Approve or Reject this request.";

        foreach (var superAdmin in superAdmins)
        {
            var notification = new Notification
            {
                NotificationTypeID = FirmAdminRequestNotificationTypeId,
                UserID = superAdmin.UserID,
                Subject = subject,
                Message = message,
                Priority = "High",
                CreatedDate = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            try
            {
                await _emailService.SendNotificationEmailAsync(superAdmin.Email, superAdmin.FullName, subject, message);
                notification.IsSent = true;
                notification.SentDate = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send immediate Firm Admin request email to Super Admin {Email}", superAdmin.Email);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
