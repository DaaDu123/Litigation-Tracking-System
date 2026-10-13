using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.PermanentDeleteUser;

public class PermanentDeleteUserCommandHandler(AppDbContext _context, ILogger<PermanentDeleteUserCommandHandler> _logger) : IRequestHandler<PermanentDeleteUserCommand, bool>
{









    public async Task<bool> Handle(PermanentDeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Permanently deleting user: {UserId}", request.UserID);


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


        if (user.UserID == request.ActingUserID)
        {
            _logger.LogWarning("User {UserId} attempted to permanently delete their own account", request.ActingUserID);
            throw new ValidationException(["You cannot delete your own account."]);
        }



        var actingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == request.ActingUserID, cancellationToken);
        var targetRole = user.GetRole();
        var actingRole = actingUser?.GetRole();
        if (actingRole == null || targetRole == null || (int)targetRole < (int)actingRole)
        {
            _logger.LogWarning("User {ActingUserId} attempted to delete higher-privileged user {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You are not authorized to delete this user."]);
        }



        if (actingUser!.FirmID != null && user.FirmID != actingUser.FirmID)
        {
            _logger.LogWarning("User {ActingUserId} attempted to delete a user from a different firm: {TargetUserId}", request.ActingUserID, request.UserID);
            throw new ValidationException(["You can only delete users within your own firm."]);
        }




        user.IsActive = false;
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");


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
