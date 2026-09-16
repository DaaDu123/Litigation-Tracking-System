using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services;
using LTSBackend.Services.Audit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Auth.ResetPassword;

public class ResetPasswordHandler(AppDbContext _context,IPasswordService _passwordService,IAuditService _auditService,
    ILogger<ResetPasswordHandler> _logger) : IRequestHandler<ResetPasswordCommand, ResetPasswordResponseDTO>
{
    // =====================================================
    // HANDLE — Anonymous, completes the forgot-password flow
    // Validates the emailed OTP code, then sets the new password,
    // rotates the security stamp, and revokes every active refresh
    // token — signing out every device, since a password reset often
    // follows a suspected compromise.
    // =====================================================
    public async Task<ResetPasswordResponseDTO> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Password reset attempt via OTP for email: {Email}", request.Email);

        var otp = await _context.UserOtps
            .FirstOrDefaultAsync(x =>
                x.Email == request.Email &&
                x.OtpCode == request.OtpCode &&
                x.Purpose == OtpPurpose.PasswordReset &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (otp == null)
        {
            _logger.LogWarning("Password reset failed: invalid or expired OTP for {Email}", request.Email);
            throw new ValidationException(["Invalid or expired OTP code. Please request a new one."]);
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email && !x.IsDeleted, cancellationToken);

        if (user == null)
        {
            _logger.LogError("Password reset failed: user not found for {Email} despite a valid OTP", request.Email);
            throw new NotFoundException("User not found.");
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        otp.IsUsed = true;

        // Rotating the security stamp + revoking refresh tokens signs out
        // every device already logged in - important since the account
        // may have been compromised (that's often *why* it's being reset).
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        var activeTokens = await _context.RefreshTokens.Where(x => x.UserID == user.UserID && !x.IsRevoked).ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        _context.AuditLogs.Add(_auditService.Create(user.UserID, "Password Reset via OTP"));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset successfully for user {UserId}; revoked {Count} active session(s)",user.UserID, activeTokens.Count);

        return new ResetPasswordResponseDTO
        {
            Email = user.Email,
            Message = "Password reset successfully! You can now login with your new password."
        };
    }
}
