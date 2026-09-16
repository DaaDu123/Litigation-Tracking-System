using LTSBackend.Comman.Enum;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LTSBackend.Features.Auth.ForgotPassword;

// OTP-based flow (matches Registration's OTP pattern) - a 6-digit code is
// emailed and consumed together with the new password in one call to
// ResetPasswordHandler. Replaces the old single-use reset-link/token
// design; see ResetPasswordHandler for the other half of this flow.
public class ForgotPasswordHandler(AppDbContext _context, IEmailService _emailService, ILogger<ForgotPasswordHandler> _logger) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponseDTO>
{
    private const int OtpExpiryMinutes = 5;

    // =====================================================
    // HANDLE — Anonymous "forgot my password" request
    // Looks up the email; if a matching active user exists, invalidates
    // any previous unused password-reset OTP and emails a fresh one
    // (also doubling as this flow's "resend" call). ALWAYS returns the
    // same generic response regardless of whether the email exists, so
    // the endpoint can't be used to enumerate registered accounts.
    // =====================================================
    public async Task<ForgotPasswordResponseDTO> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Forgot password requested for email: {Email}", request.Email);

        // Always return this, whether or not the email exists/is active -
        // prevents user enumeration via response differences.
        var genericResponse = new ForgotPasswordResponseDTO
        {
            Email = request.Email,
            Message = "If this email is registered in our system, an OTP has been sent. Please also check your spam/junk folder."
        };

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email && !x.IsDeleted, cancellationToken);

        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Forgot password: no active user for {Email} (returning generic response)", request.Email);
            return genericResponse;
        }

        // Only the most recently requested OTP should ever be valid - also
        // doubles as this endpoint's "resend" path (calling it again just
        // invalidates the old code and emails a fresh one).
        var oldOtps = await _context.UserOtps.Where(x => x.Email == request.Email && !x.IsUsed && x.Purpose == OtpPurpose.PasswordReset).ToListAsync(cancellationToken);

        if (oldOtps.Count > 0)
        {
            _context.UserOtps.RemoveRange(oldOtps);
            _logger.LogInformation("Removed {Count} old password-reset OTP(s) for {Email}", oldOtps.Count, request.Email);
        }

        string otpCode = GenerateSecureOtp();

        _context.UserOtps.Add(new UserOtp
        {
            Email = request.Email,
            OtpCode = otpCode,
            Purpose = OtpPurpose.PasswordReset,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            IsUsed = false,
            UserID = user.UserID,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Password-reset OTP generated for user: {UserId}", user.UserID);

        // Fail silently so a send failure can't be used to enumerate
        // valid accounts (same reasoning as the generic response above).
        try
        {
            await _emailService.SendOtpEmailAsync(user.Email, user.FullName, otpCode);
            _logger.LogInformation("Password-reset OTP email sent to: {Email}", request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password-reset OTP email to: {Email}", request.Email);
        }

        return genericResponse;
    }

    private static string GenerateSecureOtp() => RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
}
