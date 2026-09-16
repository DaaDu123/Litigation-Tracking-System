using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Security;
using LTSBackend.Services;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;

public class SubmitUserJoinRequestCommandHandler(AppDbContext _context, IPasswordService _passwordService, IEmailService _emailService,
    ILogger<SubmitUserJoinRequestCommandHandler> _logger) : IRequestHandler<SubmitUserJoinRequestCommand, int>
{
    // NotificationTypeID = 7 ("UserJoinRequest") - seeded in AppDbContext.SeedNotificationTypes.
    private const int UserJoinRequestNotificationTypeId = 7;

    // =====================================================
    // HANDLE — anonymous self-service request to join an existing firm
    // Confirms the target firm is real and not blocked/removed, that the
    // requested role is one of the four joinable roles, that the email
    // isn't already a live user or tied to another still-pending request
    // (to this firm or any other), hashes the password immediately
    // (plaintext is never stored), saves the request as Pending, and
    // alerts every active FirmAdmin of that specific firm (in-app
    // notification + an immediate email, not the 2-minute dispatcher poll).
    // =====================================================
    public async Task<int> Handle(SubmitUserJoinRequestCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? request.Phone : request.Phone.Trim();
        var department = string.IsNullOrWhiteSpace(request.Department) ? request.Department : request.Department.Trim();

        // 1. Target firm must exist and must currently be usable.
        var firm = await _context.Firms.AsNoTracking().FirstOrDefaultAsync(x => x.FirmID == request.FirmID, cancellationToken);

        if (firm == null || firm.IsDeleted)
            throw new ValidationException(["The selected firm could not be found."]);

        if (firm.IsBlocked)
            throw new ValidationException(["This firm workspace is currently blocked and cannot accept new join requests."]);

        // 2. Requested role must be one of the four joinable roles -
        // re-checked here too (not just in the validator) since this is
        // the actual security boundary, not just form UX.
        if (!RoleHierarchy.IsJoinableRole(request.RequestedRoleID))
            throw new ValidationException(["Invalid requested role."]);

        // 3. Email must not already belong to a live user anywhere, and
        // must not already be tied to another still-Pending join request
        // OR a still-Pending Firm Admin request.
        bool emailTaken = await _context.Users.AsNoTracking().AnyAsync(x => x.Email == email && !x.IsDeleted, cancellationToken);

        if (emailTaken)
            throw new ValidationException([$"Email '{email}' already exists."]);

        bool joinPending = await _context.UserJoinRequests.AsNoTracking().AnyAsync(x => x.Email == email && x.Status == "Pending", cancellationToken);

        if (joinPending)
            throw new ValidationException([$"A request for email '{email}' is already pending review."]);

        bool firmAdminRequestPending = await _context.FirmAdminRequests.AsNoTracking().AnyAsync(x => x.AdminEmail == email && x.Status == "Pending", cancellationToken);

        if (firmAdminRequestPending)
            throw new ValidationException([$"A Firm Admin request for email '{email}' is already pending review."]);

        // 4. Persist the pending request. Password is hashed now - the
        // plaintext is never stored - so Approve just copies the hash
        // onto the new User row.
        var joinRequest = new UserJoinRequest
        {
            FirmID = request.FirmID,
            FullName = fullName,
            Email = email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Phone = phone,
            Department = department,
            RequestedRoleID = request.RequestedRoleID,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _context.UserJoinRequests.Add(joinRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User join request {RequestId} submitted for firm {FirmId} by {Email}, requested role {RoleId}",joinRequest.RequestID, request.FirmID, email, request.RequestedRoleID);

        // 5. Alert every FirmAdmin of THIS firm - in-app Notification AND
        // an immediate email (not the 2-minute background dispatcher,
        // this needs to reach them right away).
        await NotifyFirmAdminsAsync(joinRequest, firm, cancellationToken);

        return joinRequest.RequestID;
    }

    private async Task NotifyFirmAdminsAsync(UserJoinRequest joinRequest, Firm firm, CancellationToken cancellationToken)
    {
        var firmAdmins = await _context.Users.AsNoTracking()
            .Where(x => x.FirmID == joinRequest.FirmID && x.RoleID == (int)UserRole.FirmAdmin && x.IsActive && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        if (firmAdmins.Count == 0)
        {
            _logger.LogWarning("No active Firm Admin found for firm {FirmId} to notify about join request {RequestId}", joinRequest.FirmID, joinRequest.RequestID);
            return;
        }

        var subject = "New Request to Join Your Firm";
        var message = $"{joinRequest.FullName} ({joinRequest.Email}) has requested to join \"{firm.FirmName}\" as " +
                       $"{((UserRole)joinRequest.RequestedRoleID)}. Please review and Approve or Reject this request.";

        foreach (var firmAdmin in firmAdmins)
        {
            var notification = new Notification
            {
                NotificationTypeID = UserJoinRequestNotificationTypeId,
                UserID = firmAdmin.UserID,
                Subject = subject,
                Message = message,
                Priority = "High",
                CreatedDate = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            // Send the email immediately, right here, instead of waiting
            // for NotificationEmailDispatcherService's 2-minute poll - a
            // pending join request should reach the Firm Admin's inbox
            // without delay.
            try
            {
                await _emailService.SendNotificationEmailAsync(firmAdmin.Email, firmAdmin.FullName, subject, message);
                notification.IsSent = true;
                notification.SentDate = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // Don't fail the whole request just because email delivery
                // failed - the in-app notification (and, as a fallback,
                // NotificationEmailDispatcherService's next 2-minute pass)
                // still gets it to the Firm Admin.
                _logger.LogError(ex, "Failed to send immediate join-request email to Firm Admin {Email}", firmAdmin.Email);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
