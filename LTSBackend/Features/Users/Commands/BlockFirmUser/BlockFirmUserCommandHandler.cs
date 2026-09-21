using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.BlockFirmUser;

public class BlockFirmUserCommandHandler(AppDbContext _context, ICurrentUserService _currentUser, IAuditService _auditService,
    ILogger<BlockFirmUserCommandHandler> _logger) : IRequestHandler<BlockFirmUserCommand, bool>
{
    // =====================================================
    // HANDLE — Firm Admin blocks a user in their OWN firm.
    // Tenant isolation is automatic: the Users query filter already
    // scopes this lookup to the caller's own FirmID claim, so a request
    // for a user in a different firm simply comes back NotFound - exactly
    // like trying to reach another firm's data anywhere else in this
    // codebase. Cannot block yourself or another FirmAdmin/SuperAdmin.
    // Immediately invalidates the target's current session (SecurityStamp
    // + revoke refresh tokens) so the block takes effect right away, not
    // just on their next login.
    // =====================================================
    public async Task<bool> Handle(BlockFirmUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        if (request.UserID == _currentUser.UserID)
            throw new ValidationException(["You cannot block your own account."]);

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken)
            ?? throw new NotFoundException("This user is not a member of this firm.");

        if (user.RoleID == (int)UserRole.FirmAdmin || user.RoleID == (int)UserRole.SuperAdmin)
            throw new ValidationException(["This user cannot be blocked."]);

        if (user.MembershipStatus == MembershipStatuses.Blocked)
            throw new ValidationException(["This user is already blocked."]);

        user.MembershipStatus = MembershipStatuses.Blocked;
        user.MembershipBlockedReason = request.Reason;
        user.MembershipBlockedAtUtc = DateTime.UtcNow;
        user.MembershipBlockedByUserID = _currentUser.UserID;
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        var activeTokens = await _context.RefreshTokens.Where(x => x.UserID == user.UserID && !x.IsRevoked).ToListAsync(cancellationToken);
        foreach (var token in activeTokens)
            token.IsRevoked = true;

        _context.FirmMembershipEvents.Add(new FirmMembershipEvent
        {
            FirmID = user.FirmID!.Value,
            UserID = user.UserID,
            ActionType = "Blocked",
            Reason = request.Reason,
            PerformedByUserID = _currentUser.UserID,
            PerformedAtUtc = DateTime.UtcNow
        });

        _context.AuditLogs.Add(_auditService.Create(_currentUser.UserID, $"Blocked user {user.Email} (UserID {user.UserID}): {request.Reason}"));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} blocked by {ActingUserId}", user.UserID, _currentUser.UserID);

        return true;
    }
}
