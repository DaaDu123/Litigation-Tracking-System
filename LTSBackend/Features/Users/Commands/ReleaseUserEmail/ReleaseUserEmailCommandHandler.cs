using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.ReleaseUserEmail;

public class ReleaseUserEmailCommandHandler(AppDbContext _context, ILogger<ReleaseUserEmailCommandHandler> _logger) : IRequestHandler<ReleaseUserEmailCommand, bool>
{
    // =====================================================
    // HANDLE — SuperAdmin releases a deleted user's email for cross-firm reuse
    // Only a soft-deleted, not-yet-released record can be released.
    // Doesn't touch FirmID (kept for audit history) or restore the
    // account — it purely flips IsReleasedForReuse so a DIFFERENT firm's
    // CreateUser call can claim this same email/row going forward (see
    // CreateUserCommandHandler's reuse-ownership check).
    // =====================================================
    public async Task<bool> Handle(ReleaseUserEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Release-for-reuse request for user: {UserId}", request.UserID);

        // 1. Find the user — IgnoreQueryFilters() because the row
        // we need is, by definition, soft-deleted (and possibly in
        // another firm than the caller, though only SuperAdmin can
        // reach this endpoint so tenant scoping doesn't apply here).
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Release failed: User not found: {UserId}", request.UserID);
            throw new NotFoundException("User not found.");
        }

        // 2. Only soft-deleted records can be released. A live user
        // has nothing to "release" — their email is already exactly
        // as available (i.e. not available) as it should be.
        if (!user.IsDeleted)
        {
            _logger.LogWarning("Release failed: User is not deleted: {UserId}", request.UserID);
            throw new ValidationException(["Only a deleted user's email can be released for reassignment."]);
        }

        if (user.IsReleasedForReuse)
        {
            _logger.LogWarning("Release failed: Email already released: {UserId}", request.UserID);
            throw new ValidationException(["This email has already been released for reassignment."]);
        }

        // 3. Release it. This does NOT touch FirmID (kept for audit/
        // history purposes — "who did this record originally belong
        // to") and does NOT restore the account; it purely unlocks
        // the email so a DIFFERENT firm's CreateUser call can now
        // reuse this same row (see CreateUserCommandHandler step 1).
        user.IsReleasedForReuse = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email released for reassignment for user: {UserId}", request.UserID);

        return true;
    }
}
