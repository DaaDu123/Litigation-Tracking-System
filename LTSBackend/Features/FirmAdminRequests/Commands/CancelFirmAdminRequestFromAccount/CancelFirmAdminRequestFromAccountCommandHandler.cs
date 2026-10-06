using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services.AccessRequests;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.FirmAdminRequests.Commands.CancelFirmAdminRequestFromAccount;

public class CancelFirmAdminRequestFromAccountCommandHandler(AppDbContext _context, ICurrentUserService _currentUser,
    ILogger<CancelFirmAdminRequestFromAccountCommandHandler> _logger) : IRequestHandler<CancelFirmAdminRequestFromAccountCommand, bool>
{
    // =====================================================
    // HANDLE — cancels the caller's OWN pending Firm Admin request only.
    // Checks UserID == the caller (not just RequestID) so nobody can cancel
    // someone else's request by guessing its ID. Request status change and
    // slot release are committed together in one SaveChanges.
    // =====================================================
    public async Task<bool> Handle(CancelFirmAdminRequestFromAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        var userId = _currentUser.UserID.Value;

        var firmAdminRequest = await _context.FirmAdminRequests.FirstOrDefaultAsync(x => x.RequestID == request.RequestID, cancellationToken);

        if (firmAdminRequest == null || firmAdminRequest.UserID != userId)
            throw new NotFoundException("Firm Admin request not found.");

        if (firmAdminRequest.Status != "Pending")
            throw new ValidationException([$"This request has already been {firmAdminRequest.Status.ToLower()}."]);

        firmAdminRequest.Status = "Cancelled";
        firmAdminRequest.ReviewedBy = userId;
        firmAdminRequest.ReviewedAt = DateTime.UtcNow;

        await AccessRequestSlot.ReleaseAsync(_context, userId, UserAccessRequestSlot.TargetSuperAdmin, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} cancelled their own Firm Admin request {RequestId}", userId, firmAdminRequest.RequestID);

        return true;
    }
}
