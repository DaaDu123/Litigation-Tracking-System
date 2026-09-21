using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.RemoveFirmUser;

public class RemoveFirmUserCommandHandler(AppDbContext _context, ICurrentUserService _currentUser, IAuditService _auditService,
    ILogger<RemoveFirmUserCommandHandler> _logger) : IRequestHandler<RemoveFirmUserCommand, bool>
{
    // =====================================================
    // HANDLE — detaches a user from the firm entirely (distinct from
    // Block: FirmID is cleared, freeing the user to request a different
    // firm - or, per business rule, this same firm again later). Reason
    // is mandatory and shown to the removed user, plus kept in
    // FirmMembershipEvents/AuditLogs for history.
    // =====================================================
    public async Task<bool> Handle(RemoveFirmUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        if (request.UserID == _currentUser.UserID)
            throw new ValidationException(["You cannot remove your own account."]);

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken)
            ?? throw new NotFoundException("This user is not a member of this firm.");

        if (user.RoleID == (int)UserRole.FirmAdmin || user.RoleID == (int)UserRole.SuperAdmin)
            throw new ValidationException(["This user cannot be removed."]);

        var firmId = user.FirmID!.Value;

        user.LastFirmID = firmId;
        user.FirmID = null;
        user.RoleID = null;
        user.MembershipStatus = MembershipStatuses.Removed;
        user.MembershipRemovedReason = request.Reason;
        user.MembershipRemovedAtUtc = DateTime.UtcNow;
        user.MembershipRemovedByUserID = _currentUser.UserID;
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        var activeTokens = await _context.RefreshTokens.Where(x => x.UserID == user.UserID && !x.IsRevoked).ToListAsync(cancellationToken);
        foreach (var token in activeTokens)
            token.IsRevoked = true;

        _context.FirmMembershipEvents.Add(new FirmMembershipEvent
        {
            FirmID = firmId,
            UserID = user.UserID,
            ActionType = "Removed",
            Reason = request.Reason,
            PerformedByUserID = _currentUser.UserID,
            PerformedAtUtc = DateTime.UtcNow
        });

        // In-app notification so the removed user sees the reason next
        // time they log in - never exposes anything beyond the reason
        // the Firm Admin gave.
        _context.Notifications.Add(new Notification
        {
            NotificationTypeID = 9, // "FirmMembershipChange" - seeded in AppDbContext.SeedNotificationTypes
            UserID = user.UserID,
            Subject = "You have been removed from this firm",
            Message = $"Your Firm Admin removed your membership. Reason: {request.Reason}",
            Priority = "High",
            CreatedDate = DateTime.UtcNow
        });

        _context.AuditLogs.Add(_auditService.Create(_currentUser.UserID, $"Removed user {user.Email} (UserID {user.UserID}) from firm {firmId}: {request.Reason}"));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} removed from firm {FirmId} by {ActingUserId}", user.UserID, firmId, _currentUser.UserID);

        return true;
    }
}
