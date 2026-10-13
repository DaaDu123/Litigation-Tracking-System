using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Users.Commands.DeleteUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler(AppDbContext _context, ILogger<DeleteUserCommandHandler> _logger) : IRequestHandler<DeleteUserCommand, bool>
{









    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating user (reversible): {UserId}", request.UserID);


        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Delete failed: User not found: {UserId}", request.UserID);
            throw new NotFoundException("User not found.");
        }


        if (user.IsDeleted)
        {
            _logger.LogWarning("Delete failed: User already deleted: {UserId}", request.UserID);
            throw new ValidationException(["User account is already deleted."]);
        }

        if (user.UserID == request.ActingUserID)
        {
            _logger.LogWarning("User {UserId} attempted to deactivate their own account", request.ActingUserID);
            throw new ValidationException(["You cannot deactivate your own account."]);
        }


        var actingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == request.ActingUserID, cancellationToken);
        var targetRole = user.GetRole();
        var actingRole = actingUser?.GetRole();
        if (actingRole == null || targetRole == null || (int)targetRole < (int)actingRole)
        {
            _logger.LogWarning("User {ActingUserId} attempted to deactivate higher-privileged user {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You are not authorized to deactivate this user."]);
        }



        if (actingUser!.FirmID != null && user.FirmID != actingUser.FirmID)
        {
            _logger.LogWarning("User {ActingUserId} attempted to deactivate a user from a different firm: {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You can only deactivate users within your own firm."]);
        }


        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");


        var activeTokens = await _context.RefreshTokens.Where(x => x.UserID == request.UserID && !x.IsRevoked).ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        _logger.LogInformation("Revoked {Count} active tokens for user: {UserId}", activeTokens.Count, request.UserID);


        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User deactivated successfully: {UserId}", request.UserID);

        return true;
    }
}