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
    // =====================================================
    // HANDLE — Firm Admin changes a firm user's role among
    // Partner / AssociateLawyer / Moharrir / InternParalegal. Firm-scoped
    // automatically via the Users tenant query filter (a FirmAdmin from
    // Firm A cannot even load a Firm B user, let alone change their
    // role). Reuses RoleHierarchy.CanAssignRole - the same rule that
    // stops a FirmAdmin creating another FirmAdmin also stops this from
    // ever assigning FirmAdmin/SuperAdmin.
    // =====================================================
    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            throw new UnauthorizedException("You must be logged in.");

        if (request.UserID == _currentUser.UserID)
            throw new ValidationException(["You cannot change your own role."]);

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID, cancellationToken)
            ?? throw new NotFoundException("This user is not a member of this firm.");

        if (!RoleHierarchy.CanAssignRole(UserRole.FirmAdmin, request.NewRoleID))
            throw new ValidationException(["That role cannot be assigned."]);

        var oldRoleId = user.RoleID;
        user.RoleID = request.NewRoleID;

        _context.AuditLogs.Add(_auditService.Create(_currentUser.UserID,
            $"Changed role for user {user.Email} (UserID {user.UserID}) from {oldRoleId} to {(UserRole)request.NewRoleID}"));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} role changed to {NewRoleId} by {ActingUserId}", user.UserID, request.NewRoleID, _currentUser.UserID);

        return true;
    }
}
