using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.UnblockFirmUser;

public class UnblockFirmUserCommandHandler(AppDbContext _context, ICurrentUserService _currentUser, IAuditService _auditService,
    ILogger<UnblockFirmUserCommandHandler> _logger) : IRequestHandler<UnblockFirmUserCommand, bool>
{
    // =====================================================
    // HANDLE — restores a Blocked user's access. Tenant-scoped
    // automatically via the Users query filter, same as Block.
    // =====================================================
    public async Task<bool> Handle(UnblockFirmUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken)
            ?? throw new NotFoundException("This user is not a member of this firm.");

        if (user.MembershipStatus != MembershipStatuses.Blocked)
            throw new ValidationException(["This user is not currently blocked."]);

        user.MembershipStatus = MembershipStatuses.Active;
        user.MembershipBlockedReason = null;
        user.MembershipBlockedAtUtc = null;
        user.MembershipBlockedByUserID = null;

        _context.FirmMembershipEvents.Add(new FirmMembershipEvent
        {
            FirmID = user.FirmID!.Value,
            UserID = user.UserID,
            ActionType = "Unblocked",
            PerformedByUserID = _currentUser.UserID,
            PerformedAtUtc = DateTime.UtcNow
        });

        _context.AuditLogs.Add(_auditService.Create(_currentUser.UserID, $"Unblocked user {user.Email} (UserID {user.UserID})"));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} unblocked by {ActingUserId}", user.UserID, _currentUser.UserID);

        return true;
    }
}
