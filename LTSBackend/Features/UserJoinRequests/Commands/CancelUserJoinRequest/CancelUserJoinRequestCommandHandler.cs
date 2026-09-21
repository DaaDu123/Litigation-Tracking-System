using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Commands.CancelUserJoinRequest;

public class CancelUserJoinRequestCommandHandler(AppDbContext _context, ICurrentUserService _currentUser,
    ILogger<CancelUserJoinRequestCommandHandler> _logger) : IRequestHandler<CancelUserJoinRequestCommand, bool>
{
    // =====================================================
    // HANDLE — cancels the caller's OWN pending join request only.
    // Deliberately checks UserID == the caller (not just RequestID) so a
    // Firm User cannot cancel someone else's request by guessing its ID.
    // =====================================================
    public async Task<bool> Handle(CancelUserJoinRequestCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        var joinRequest = await _context.UserJoinRequests
            .IgnoreQueryFilters() // UserJoinRequest's tenant filter is firm-scoped; the requester has no FirmID yet
            .FirstOrDefaultAsync(x => x.RequestID == request.RequestID, cancellationToken);

        if (joinRequest == null || joinRequest.UserID != _currentUser.UserID)
            throw new NotFoundException("Join request not found.");

        if (joinRequest.Status != "Pending")
            throw new ValidationException([$"This request has already been {joinRequest.Status.ToLower()}."]);

        joinRequest.Status = "Cancelled";
        joinRequest.ReviewedBy = _currentUser.UserID;
        joinRequest.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} cancelled their own join request {RequestId}", _currentUser.UserID, joinRequest.RequestID);

        return true;
    }
}
