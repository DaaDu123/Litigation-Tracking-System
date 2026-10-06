using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Security;
using LTSBackend.Services.AccessRequests;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequestFromAccount;

public class SubmitFirmAdminRequestFromAccountCommandHandler(AppDbContext _context, ICurrentUserService _currentUser, IEmailService _emailService,
    ILogger<SubmitFirmAdminRequestFromAccountCommandHandler> _logger) : IRequestHandler<SubmitFirmAdminRequestFromAccountCommand, int>
{
    // NotificationTypeID = 6 ("FirmAdminRequest") - seeded in AppDbContext.SeedNotificationTypes. Same one used by the anonymous flow.
    private const int FirmAdminRequestNotificationTypeId = 6;

    public async Task<int> Handle(SubmitFirmAdminRequestFromAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in to request Firm Admin access.");

        if (_currentUser.IsSuperAdmin)
            throw new ValidationException(["Super Admin cannot create a firm workspace for itself."]);

        var userId = _currentUser.UserID.Value;

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (user.FirmID.HasValue)
            throw new ValidationException(["You are already a member of a firm."]);

        bool alreadyPending = await _context.FirmAdminRequests.AsNoTracking()
            .AnyAsync(x => (x.UserID == userId || x.AdminEmail == user.Email) && x.Status == "Pending", cancellationToken);

        if (alreadyPending)
            throw new ValidationException(["You already have a pending Firm Admin request."]);

        bool joinPending = await _context.UserJoinRequests.AsNoTracking().IgnoreQueryFilters()
            .AnyAsync(x => x.UserID == userId && x.Status == "Pending", cancellationToken);

        if (joinPending)
            throw new ValidationException(["You have already sent an access request to a Firm Admin. You can only have one access request at a time, so you cannot request the Super Admin. Cancel your existing request first."]);

        var firmAdminRequest = new FirmAdminRequest
        {
            UserID = user.UserID,
            AdminEmail = user.Email,
            AdminFullName = user.FullName,
            AdminPhone = user.Phone,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _context.FirmAdminRequests.Add(firmAdminRequest);

        // DB-level guarantee (UserAccessRequestSlots PK) - see SubmitUserJoinRequestCommandHandler.
        await AccessRequestSlot.AcquireAsync(_context, userId, UserAccessRequestSlot.TargetSuperAdmin, cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (AccessRequestSlot.IsSlotConflict(ex))
        {
            throw new ValidationException([AccessRequestSlot.ConflictMessage]);
        }

        _logger.LogInformation("Firm Admin request {RequestId} submitted from account by user {UserId} ({Email})", firmAdminRequest.RequestID, userId, user.Email);

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
