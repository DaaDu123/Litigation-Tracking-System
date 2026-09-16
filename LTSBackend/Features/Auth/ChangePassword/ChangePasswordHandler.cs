using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Middleware;
using LTSBackend.Data;
using LTSBackend.Features.Auth.ChangePassword;
using LTSBackend.Services;
using LTSBackend.Services.Audit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Auth.ChangePassword;

public class ChangePasswordHandler (AppDbContext _context, IPasswordService _passwordService, IAuditService _auditService, ILogger<ChangePasswordHandler> _logger) : IRequestHandler<ChangePasswordCommand, bool>
{

    // =====================================================
    // HANDLE — Any authenticated user changing their own password
    // Verifies the old password, hashes and saves the new one, then
    // rotates the user's security stamp and revokes every active
    // refresh token so all other logged-in devices/sessions are signed
    // out. Writes an audit log entry.
    // =====================================================
    public async Task<bool> Handle(ChangePasswordCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Password change attempt for user: {UserId}", request.UserID);

        // 1. Find user
        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == request.UserID,cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Password change failed: User not found: {UserId}", request.UserID);
            throw new NotFoundException("User not found.");
        }

        // 2. Verify old password
        bool isOldPasswordValid = _passwordService.VerifyPassword(request.OldPassword,user.PasswordHash);

        if (!isOldPasswordValid)
        {
            _logger.LogWarning("Password change failed: Invalid old password for user: {UserId}", request.UserID);
            throw new ValidationException(new List<string> { "Old password is incorrect." });
        }

        // 3. Update password
        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;  // FIX: Use UpdatedAt instead of non-existent PasswordChangedDate

        user.SecurityStamp = Guid.NewGuid().ToString("N");

        var activeTokens = await _context.RefreshTokens
            .Where(x => x.UserID == user.UserID && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        _logger.LogInformation("Rotated security stamp and revoked {Count} active session(s) for user {UserId} after password change",activeTokens.Count,user.UserID);

        // 4. Create audit log
        _context.AuditLogs.Add(_auditService.Create(user.UserID, "Password Changed"));

        // 5. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserID);

        return true;
    }
}