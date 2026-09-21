using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;

public class SubmitUserJoinRequestCommandHandler(AppDbContext _context, ICurrentUserService _currentUser,
    ILogger<SubmitUserJoinRequestCommandHandler> _logger) : IRequestHandler<SubmitUserJoinRequestCommand, int>
{
    // =====================================================
    // HANDLE — an already-registered, profile-completed Firm User sends
    // ONE request to join a firm, from their own dashboard.
    //
    // Enforces, at the backend (never trusting the frontend to have
    // already checked these):
    //   - the caller must be a real, authenticated user (not SuperAdmin -
    //     they don't join firms);
    //   - the target firm must exist and not be blocked/deleted;
    //   - the caller must not already belong to a firm (one firm per
    //     user - "cannot join multiple firms");
    //   - the caller must not already have another Pending request
    //     outstanding ("one active request at a time");
    //   - the caller must not currently be Blocked from that specific
    //     firm (checked via FirmMembershipEvents history, since a block
    //     record survives even if the user was later fully Removed).
    // =====================================================
    public async Task<int> Handle(SubmitUserJoinRequestCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in to request a firm.");

        var userId = _currentUser.UserID.Value;

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User not found.");

        if (user.FirmID.HasValue)
            throw new ValidationException(["You are already a member of a firm."]);

        var firm = await _context.Firms.AsNoTracking().FirstOrDefaultAsync(x => x.FirmID == request.FirmID && !x.IsDeleted, cancellationToken);

        if (firm == null)
            throw new NotFoundException("Firm not found.");

        if (firm.IsBlocked)
            throw new ValidationException(["This firm is not currently accepting requests."]);

        bool alreadyPending = await _context.UserJoinRequests.AsNoTracking().AnyAsync(x => x.UserID == userId && x.Status == "Pending", cancellationToken);

        if (alreadyPending)
            throw new ValidationException(["You already have a pending request. Cancel it before requesting another firm."]);

        // A live Blocked record (no later Unblocked event) for this
        // specific user+firm pair means "cannot re-request this firm".
        // Being merely Removed does not block re-requesting.
        var lastEventForFirm = await _context.FirmMembershipEvents.AsNoTracking().IgnoreQueryFilters()
            .Where(x => x.UserID == userId && x.FirmID == request.FirmID)
            .OrderByDescending(x => x.PerformedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastEventForFirm?.ActionType == "Blocked")
            throw new ValidationException(["You are blocked from this firm and cannot request to join it."]);

        var joinRequest = new UserJoinRequest
        {
            FirmID = request.FirmID,
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _context.UserJoinRequests.Add(joinRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} submitted a join request {RequestId} for firm {FirmId}", userId, joinRequest.RequestID, request.FirmID);

        return joinRequest.RequestID;
    }
}
