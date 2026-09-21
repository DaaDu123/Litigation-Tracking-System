using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LTSBackend.Features.Auth.Register;

public class RegisterHandler(AppDbContext _context, IPasswordService _passwordService, IEmailService _emailService, IAuditService _auditService, ILogger<RegisterHandler> _logger) : IRequestHandler<RegisterCommand, RegisterResponseDTO>
{
    // =====================================================
    // HANDLE — Anonymous Firm User self-registration (Email + Password only)
    // Creates the account immediately with NO firm and NO role
    // (FirmID = null, RoleID = null) - it becomes a real row right away,
    // rather than a pending request, because a Firm User needs to log in,
    // land on their own dashboard, and complete their profile BEFORE they
    // ever pick a firm to request. IsActive stays false until the emailed
    // Registration OTP is verified (VerifyOtpHandler), matching the
    // existing pattern used everywhere else in this codebase.
    //
    // IMPORTANT - idempotent retry for a "stuck" unverified account:
    // if the OTP email failed to send the first time (SMTP misconfigured,
    // provider hiccup, etc.), the user is left with a real, unverified
    // row and no way to reach a "resend" screen from Register itself -
    // registering again with the same email would otherwise dead-end on
    // "Email already exists." Instead, when the existing row for this
    // email is still unverified (IsActive == false), this quietly treats
    // the retry as a resend: refreshes the password (in case they mistyped
    // it originally) and issues a fresh OTP, rather than blocking. Only a
    // genuinely ACTIVE (already-verified) account blocks re-registration.
    // =====================================================
    public async Task<RegisterResponseDTO> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Firm User registration for email: {Email}", request.Email);

        var email = request.Email?.Trim() ?? string.Empty;

        var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted, cancellationToken);

        if (existingUser != null && existingUser.IsActive)
        {
            _logger.LogWarning("Registration failed: Email already exists and is verified: {Email}", email);
            throw new ValidationException(["Email already exists."]);
        }

        bool joinPending = await _context.UserJoinRequests.AsNoTracking().AnyAsync(x => x.Email == email && x.Status == "Pending", cancellationToken);
        if (joinPending)
            throw new ValidationException(["A request for this email is already pending review."]);

        bool firmAdminPending = await _context.FirmAdminRequests.AsNoTracking().AnyAsync(x => x.AdminEmail == email && x.Status == "Pending", cancellationToken);
        if (firmAdminPending)
            throw new ValidationException(["A Firm Admin request for this email is already pending review."]);

        User user;

        if (existingUser != null)
        {
            // Unverified row from a previous attempt whose OTP email never
            // arrived - reuse it instead of creating a duplicate/blocking.
            existingUser.PasswordHash = _passwordService.HashPassword(request.Password);
            user = existingUser;
            _logger.LogInformation("Re-registering unverified account (previous OTP email likely failed): {UserId}", user.UserID);
        }
        else
        {
            // 2. Create the user account - no firm, no role, profile
            // incomplete, until they complete their profile and are later
            // accepted into a firm via UserJoinRequest.
            var placeholderName = email.Split('@')[0];

            user = new User
            {
                EmployeeNo = GenerateEmployeeNo(),
                FullName = placeholderName,
                Email = email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                RoleID = null,
                FirmID = null,
                IsActive = false, // becomes true once the Registration OTP is verified
                IsProfileCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Firm User account ready (pending email verification): {UserId}", user.UserID);

        // 3. Invalidate any previous unused Registration OTP for this
        // email (mirrors ResendOtpHandler) before issuing a fresh one.
        var oldOtps = await _context.UserOtps.Where(x => x.Email == email && !x.IsUsed && x.Purpose == OtpPurpose.Registration).ToListAsync(cancellationToken);
        if (oldOtps.Count > 0)
            _context.UserOtps.RemoveRange(oldOtps);

        string otpCode = GenerateSecureOtp();

        var userOtp = new UserOtp
        {
            Email = email,
            OtpCode = otpCode,
            Purpose = OtpPurpose.Registration,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            UserID = user.UserID,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserOtps.Add(userOtp);

        var auditLog = _auditService.Create(user.UserID, "Firm User Registered (profile/firm pending)");
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendOtpEmailAsync(email, user.FullName, otpCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to: {Email}", email);

            // IMPORTANT: do NOT throw here. The account and OTP row above
            // are already saved either way, and the Register page itself
            // has no "Resend OTP" action - only the verify-otp screen
            // does. Throwing would strand the user on Register with no
            // way forward. Instead, let them proceed to verify-otp (same
            // as the success path) where they can use its working Resend
            // OTP button once the email problem is fixed - the response
            // message below is adjusted so it doesn't falsely claim the
            // email was sent.
            return new RegisterResponseDTO
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Message = "Your account is ready, but we couldn't send the verification email just now. " +
                          "Please use \"Resend OTP\" on the next screen to try again."
            };
        }

        return new RegisterResponseDTO
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Message = "Registration successful! Please check your email (including Spam/Junk folder) for the OTP code to verify your account. " +
                      "After verifying, you'll be asked to complete your profile before you can request to join a firm."
        };
    }

    private static string GenerateSecureOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
    }

    private static string GenerateEmployeeNo()
    {
        var now = DateTime.UtcNow;
        var random = RandomNumberGenerator.GetInt32(1000, 9999);
        return $"EMP-{now:yyyyMMdd}-{random}";
    }
}