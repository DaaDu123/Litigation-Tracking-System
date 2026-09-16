using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Users.Commands.DeleteUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler(AppDbContext _context, ILogger<DeleteUserCommandHandler> _logger) : IRequestHandler<DeleteUserCommand, bool>
{
    // =====================================================
    // HANDLE — soft-deletes/deactivates a user (reversible via Activate)
    // Refuses to deactivate yourself, and enforces role hierarchy (can
    // only deactivate a strictly lower-privileged role) plus own-firm
    // scoping (SuperAdmin bypasses, since FirmID is null for them). Sets
    // IsActive = false, rotates the security stamp, and revokes every
    // active refresh token so the user is signed out everywhere
    // immediately, not just blocked from future logins.
    // =====================================================
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating user (reversible): {UserId}", request.UserID);

        // 1. Find user
        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Delete failed: User not found: {UserId}", request.UserID);
            throw new NotFoundException("User not found.");
        }

        // 2. Check if already deleted
        if (user.IsDeleted)
        {
            _logger.LogWarning("Delete failed: User already deleted: {UserId}", request.UserID);
            throw new ValidationException(["User account is already deleted."]);
        }
        // 2b. Self-protection
        if (user.UserID == request.ActingUserID)
        {
            _logger.LogWarning("User {UserId} attempted to deactivate their own account", request.ActingUserID);
            throw new ValidationException(["You cannot deactivate your own account."]);
        }
        // 2c. Hierarchy check — can only deactivate users
        // whose role is below the acting user's own role
        var actingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == request.ActingUserID, cancellationToken);
        var targetRole = user.GetRole();
        var actingRole = actingUser?.GetRole();
        if (actingRole == null || targetRole == null || (int)targetRole < (int)actingRole)
        {
            _logger.LogWarning("User {ActingUserId} attempted to deactivate higher-privileged user {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You are not authorized to deactivate this user."]);
        }

        // 2d. Multi-tenancy: can only deactivate users in your own
        // firm (SuperAdmin, FirmID == null, bypasses this check)
        if (actingUser!.FirmID != null && user.FirmID != actingUser.FirmID)
        {
            _logger.LogWarning("User {ActingUserId} attempted to deactivate a user from a different firm: {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You can only deactivate users within your own firm."]);
        }

        // 3. Perform DEACTIVATION only (reversible)
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        // 4. Revoke all active refresh tokens
        var activeTokens = await _context.RefreshTokens.Where(x => x.UserID == request.UserID && !x.IsRevoked).ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        _logger.LogInformation("Revoked {Count} active tokens for user: {UserId}", activeTokens.Count, request.UserID);

        // 5. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User deactivated successfully: {UserId}", request.UserID);

        return true;
    }
}