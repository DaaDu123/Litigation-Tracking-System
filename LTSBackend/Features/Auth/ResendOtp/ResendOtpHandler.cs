using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Auth.ResendOtp;
using LTSBackend.Models.Security;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LTSBackend.Features.Auth.ResendOtp;

public class ResendOtpHandler(AppDbContext _context, IEmailService _emailService, ILogger<ResendOtpHandler> _logger) : IRequestHandler<ResendOtpCommand, ResendOtpResponseDTO>
{
    // =====================================================
    // HANDLE — Anonymous, Registration-flow OTP resend only
    // Invalidates any previous unused Registration OTP for the email
    // and emails a fresh one. Scoped strictly to Purpose ==
    // Registration so it never touches a separately in-flight
    // password-reset OTP for the same email.
    // =====================================================
    public async Task<ResendOtpResponseDTO> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resend OTP requested for email: {Email}", request.Email);

        // 1. Find user by email
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Resend OTP failed: User not found for email: {Email}", request.Email);
            throw new NotFoundException("User not found.");
        }

        // 2. Remove old unused OTPs
        // FIX: This endpoint is exclusively for the Registration flow
        // (grouped under "REGISTRATION & EMAIL VERIFICATION" in the
        // controller). Previously it removed ALL unused OTPs for the
        // email regardless of Purpose — if the user had also
        // requested a password-reset OTP, this would silently wipe
        // it out. Now scoped to Purpose == Registration only.
        var oldOtps = await _context.UserOtps.Where(x => x.Email == request.Email && !x.IsUsed && x.Purpose == OtpPurpose.Registration).ToListAsync(cancellationToken);

        if (oldOtps.Count > 0)
        {
            _context.UserOtps.RemoveRange(oldOtps);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Removed {Count} old registration OTPs for resend", oldOtps.Count);
        }

        // 3. Generate new 6-digit OTP
        string otpCode = GenerateSecureOtp();
        _logger.LogInformation("New OTP generated for {Email}", request.Email);

        // 4. Save new OTP
        // FIX: Purpose was previously never set, so VerifyOtpHandler's
        // filter (Purpose == OtpPurpose.Registration) could fail to
        // find this OTP depending on the enum's default value.
        _context.UserOtps.Add(new UserOtp
        {
            Email = request.Email,
            OtpCode = otpCode,
            Purpose = OtpPurpose.Registration,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            UserID = user.UserID,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("New OTP saved for user: {UserId}", user.UserID);

        // 5. Send OTP email
        try
        {
            _logger.LogInformation("Sending OTP email to: {Email}", request.Email);
            await _emailService.SendOtpEmailAsync(user.Email, user.FullName, otpCode);
            _logger.LogInformation("OTP email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to: {Email}", request.Email);
            throw;
        }

        return new ResendOtpResponseDTO
        {
            Email = user.Email,
            Message = "OTP sent successfully! Please check your email (including Spam/Junk folder) for the new code."
        };
    }
    private static string GenerateSecureOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
    }
}