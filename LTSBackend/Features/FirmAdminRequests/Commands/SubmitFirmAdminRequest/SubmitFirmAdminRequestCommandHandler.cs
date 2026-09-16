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
    // HANDLE — anonymous self-service request for a new firm workspace
    // Checks the FirmCode and admin email aren't already a live
    // firm/user OR already tied to another still-pending request,
    // hashes the password immediately (plaintext is never stored),
    // saves the request as Pending, and alerts every active SuperAdmin
    // (in-app notification + an immediate email, not the 2-minute
    // dispatcher poll).
    // =====================================================
    public async Task<int> Handle(SubmitFirmAdminRequestCommand request, CancellationToken cancellationToken)
    {
        var firmCode = request.FirmCode.Trim().ToUpperInvariant();
        var adminEmail = request.AdminEmail.Trim();
        var firmName = request.FirmName?.Trim() ?? string.Empty;
        var address = string.IsNullOrWhiteSpace(request.Address) ? request.Address : request.Address.Trim();
        var contactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? request.ContactEmail : request.ContactEmail.Trim();
        var contactPhone = string.IsNullOrWhiteSpace(request.ContactPhone) ? request.ContactPhone : request.ContactPhone.Trim();
        var adminFullName = request.AdminFullName?.Trim() ?? string.Empty;
        var adminPhone = string.IsNullOrWhiteSpace(request.AdminPhone) ? request.AdminPhone : request.AdminPhone.Trim();

        // 1. Firm code must not already belong to a live firm, and must not
        // already be tied to another still-Pending request.
        bool firmCodeTaken = await _context.Firms.AsNoTracking().AnyAsync(x => x.FirmCode == firmCode, cancellationToken);

        if (firmCodeTaken)
            throw new ValidationException([$"Firm code '{firmCode}' is already in use."]);

        bool firmCodePending = await _context.FirmAdminRequests.AsNoTracking().AnyAsync(x => x.FirmCode == firmCode && x.Status == "Pending", cancellationToken);

        if (firmCodePending)
            throw new ValidationException([$"A request for firm code '{firmCode}' is already pending Super Admin review."]);

        // 2. Admin email must not already exist as a real user, and must
        // not already be tied to another still-Pending request.
        bool emailTaken = await _context.Users.AsNoTracking().AnyAsync(x => x.Email == adminEmail, cancellationToken);

        if (emailTaken)
            throw new ValidationException([$"Email '{adminEmail}' already exists."]);

        bool emailPending = await _context.FirmAdminRequests.AsNoTracking().AnyAsync(x => x.AdminEmail == adminEmail && x.Status == "Pending", cancellationToken);

        if (emailPending)
            throw new ValidationException([$"A request for email '{adminEmail}' is already pending Super Admin review."]);

        // 3. Persist the pending request. Password is hashed now - the
        // plaintext is never stored - so Approve just copies the hash
        // onto the new User row.
        var firmAdminRequest = new FirmAdminRequest
        {
            FirmName = firmName,
            FirmCode = firmCode,
            Address = address,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            AdminFullName = adminFullName,
            AdminEmail = adminEmail,
            AdminPasswordHash = _passwordService.HashPassword(request.AdminPassword),
            AdminPhone = adminPhone,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _context.FirmAdminRequests.Add(firmAdminRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Firm Admin request {RequestId} submitted for firm code {FirmCode} by {AdminEmail}",firmAdminRequest.RequestID, firmCode, adminEmail);

        // 4. Alert every Super Admin - in-app Notification AND an
        // immediate email (not the 2-minute background dispatcher,
        // this needs to reach them right away).
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
        var message = $"{firmAdminRequest.AdminFullName} ({firmAdminRequest.AdminEmail}) has requested to create a " +
                       $"new firm workspace \"{firmAdminRequest.FirmName}\" (code: {firmAdminRequest.FirmCode}) and " +
                       $"become its Firm Admin. Please review and Approve or Reject this request.";

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

            // Send the email immediately, right here, instead of waiting
            // for NotificationEmailDispatcherService's 2-minute poll - a
            // pending Firm Admin request should reach the Super Admin's
            // inbox without delay.
            try
            {
                await _emailService.SendNotificationEmailAsync(superAdmin.Email, superAdmin.FullName, subject, message);
                notification.IsSent = true;
                notification.SentDate = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // Don't fail the whole request just because email delivery
                // failed - the in-app notification (and, as a fallback,
                // NotificationEmailDispatcherService's next 2-minute pass)
                // still gets it to the Super Admin.
                _logger.LogError(ex, "Failed to send immediate Firm Admin request email to Super Admin {Email}", superAdmin.Email);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
