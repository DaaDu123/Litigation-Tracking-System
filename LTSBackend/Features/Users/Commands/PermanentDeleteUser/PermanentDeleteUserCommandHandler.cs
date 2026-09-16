using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.PermanentDeleteUser;

public class PermanentDeleteUserCommandHandler(AppDbContext _context, ILogger<PermanentDeleteUserCommandHandler> _logger) : IRequestHandler<PermanentDeleteUserCommand, bool>
{
    // =====================================================
    // HANDLE — soft-deletes a user, freeing their email for reuse
    // Despite the name, this sets IsDeleted = true rather than removing
    // the row (CreateUserCommandHandler's uniqueness check excludes
    // IsDeleted rows, which is what actually frees the email up — see
    // ReleaseUserEmailCommand for the cross-firm reuse case). Same
    // self-protection, hierarchy, and own-firm scoping rules as
    // DeleteUserCommandHandler, plus revokes every active refresh token.
    // =====================================================
    public async Task<bool> Handle(PermanentDeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Permanently deleting user: {UserId}", request.UserID);

        // 1. Find user
        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Permanent delete failed: User not found: {UserId}", request.UserID);
            throw new NotFoundException("User not found.");
        }

        if (user.IsDeleted)
        {
            _logger.LogWarning("Permanent delete failed: User already deleted: {UserId}", request.UserID);
            throw new ValidationException(["User account is already deleted."]);
        }

        // 2. Self-protection
        if (user.UserID == request.ActingUserID)
        {
            _logger.LogWarning("User {UserId} attempted to permanently delete their own account", request.ActingUserID);
            throw new ValidationException(["You cannot delete your own account."]);
        }

        // 3. Hierarchy check — same rule as Deactivate: can only
        // remove users whose role is below the acting user's own role
        var actingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == request.ActingUserID, cancellationToken);
        var targetRole = user.GetRole();
        var actingRole = actingUser?.GetRole();
        if (actingRole == null || targetRole == null || (int)targetRole < (int)actingRole)
        {
            _logger.LogWarning("User {ActingUserId} attempted to delete higher-privileged user {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You are not authorized to delete this user."]);
        }

        // 4. Multi-tenancy: can only delete users in your own firm
        // (SuperAdmin, FirmID == null, bypasses this check)
        if (actingUser!.FirmID != null && user.FirmID != actingUser.FirmID)
        {
            _logger.LogWarning("User {ActingUserId} attempted to delete a user from a different firm: {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You can only delete users within your own firm."]);
        }

        // 5. Perform PERMANENT delete
        // This is what actually frees the email up for reuse - CreateUser's
        // uniqueness check excludes rows where IsDeleted == true.
        user.IsActive = false;
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        // 6. Revoke all active refresh tokens
        var activeTokens = await _context.RefreshTokens.Where(x => x.UserID == request.UserID && !x.IsRevoked).ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User permanently deleted: {UserId}", request.UserID);

        return true;
    }
}
