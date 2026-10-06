using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services.AccessRequests;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;

public class SubmitUserJoinRequestCommandHandler(AppDbContext _context, ICurrentUserService _currentUser,
    ILogger<SubmitUserJoinRequestCommandHandler> _logger) : IRequestHandler<SubmitUserJoinRequestCommand, int>
{
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

        bool alreadyPending = await _context.UserJoinRequests.AsNoTracking().IgnoreQueryFilters()
            .AnyAsync(x => x.UserID == userId && x.Status == "Pending", cancellationToken);

        if (alreadyPending)
            throw new ValidationException(["You already have a pending request. Cancel it before requesting another firm."]);

        bool superAdminRequestPending = await _context.FirmAdminRequests.AsNoTracking()
            .AnyAsync(x => (x.UserID == userId || x.AdminEmail == user.Email) && x.Status == "Pending", cancellationToken);

        if (superAdminRequestPending)
            throw new ValidationException(["You have already sent an access request to the Super Admin. You can only have one access request at a time, so you cannot request a Firm Admin."]);

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

        // DB-level guarantee (UserAccessRequestSlots PK): the slot and the request
        // commit in ONE SaveChanges, so even two simultaneous submissions (to
        // different Firm Admins, or Firm Admin + Super Admin) can never both succeed.
        await AccessRequestSlot.AcquireAsync(_context, userId, UserAccessRequestSlot.TargetFirmAdmin, cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (AccessRequestSlot.IsSlotConflict(ex))
        {
            throw new ValidationException([AccessRequestSlot.ConflictMessage]);
        }

        _logger.LogInformation("User {UserId} submitted a join request {RequestId} for firm {FirmId}", userId, joinRequest.RequestID, request.FirmID);

        return joinRequest.RequestID;
    }
}
