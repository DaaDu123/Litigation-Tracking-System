using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandHandler(AppDbContext _context, ICurrentUserService _currentUser, IAuditService _auditService,
    ILogger<ChangeUserRoleCommandHandler> _logger) : IRequestHandler<ChangeUserRoleCommand, bool>
{

















    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        if (request.UserID == _currentUser.UserID)
            throw new ValidationException(["You cannot change your own role."]);

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken)
            ?? throw new NotFoundException("This user is not a member of this firm.");

        if (user.RoleID == (int)UserRole.FirmAdmin || user.RoleID == (int)UserRole.SuperAdmin)
            throw new ValidationException(["This user's role cannot be changed."]);

        if (!RoleHierarchy.CanAssignRole(UserRole.FirmAdmin, request.NewRoleID))
            throw new ValidationException(["That role cannot be assigned."]);

        var oldRoleId = user.RoleID;
        user.RoleID = request.NewRoleID;
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        var activeTokens = await _context.RefreshTokens.Where(x => x.UserID == user.UserID && !x.IsRevoked).ToListAsync(cancellationToken);
        foreach (var token in activeTokens)
            token.IsRevoked = true;

        _context.AuditLogs.Add(_auditService.Create(_currentUser.UserID,
            $"Changed role for user {user.Email} (UserID {user.UserID}) from {oldRoleId} to {(UserRole)request.NewRoleID}"));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} role changed to {NewRoleId} by {ActingUserId}", user.UserID, request.NewRoleID, _currentUser.UserID);

        return true;
    }
}
