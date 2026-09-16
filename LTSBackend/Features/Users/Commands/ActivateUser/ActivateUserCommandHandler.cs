using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.ActivateUser;

public class ActivateUserCommandHandler(AppDbContext _context, ILogger<ActivateUserCommandHandler> _logger) : IRequestHandler<ActivateUserCommand, bool>
{
    // =====================================================
    // HANDLE — reverses Deactivate, re-enabling a user to log in again
    // Refuses if the user was permanently deleted (not reversible — must
    // be re-added as a new user), or is already active. Same hierarchy +
    // own-firm scoping rule as DeleteUserCommandHandler.
    // =====================================================
    public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating user: {UserId}", request.UserID);

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Activate failed: User not found: {UserId}", request.UserID);
            throw new NotFoundException("User not found.");
        }

        if (user.IsDeleted)
        {
            _logger.LogWarning("Activate failed: User is permanently deleted: {UserId}", request.UserID);
            throw new ValidationException(["This user was permanently deleted and cannot be reactivated. Add them again as a new user instead."]);
        }

        if (user.IsActive)
        {
            _logger.LogWarning("Activate failed: User is already active: {UserId}", request.UserID);
            throw new ValidationException(["User account is already active."]);
        }

        // Hierarchy + multi-tenancy checks, same rule as Deactivate
        var actingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == request.ActingUserID, cancellationToken);
        var targetRole = user.GetRole();
        var actingRole = actingUser?.GetRole();
        if (actingRole == null || targetRole == null || (int)targetRole < (int)actingRole)
        {
            _logger.LogWarning("User {ActingUserId} attempted to activate higher-privileged user {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You are not authorized to activate this user."]);
        }

        if (actingUser!.FirmID != null && user.FirmID != actingUser.FirmID)
        {
            _logger.LogWarning("User {ActingUserId} attempted to activate a user from a different firm: {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You can only activate users within your own firm."]);
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User activated successfully: {UserId}", request.UserID);

        return true;
    }
}
