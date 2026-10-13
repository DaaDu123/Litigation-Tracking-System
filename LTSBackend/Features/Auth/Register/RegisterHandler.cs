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


            existingUser.PasswordHash = _passwordService.HashPassword(request.Password);
            user = existingUser;
            _logger.LogInformation("Re-registering unverified account (previous OTP email likely failed): {UserId}", user.UserID);
        }
        else
        {



            var placeholderName = email.Split('@')[0];

            user = new User
            {
                EmployeeNo = GenerateEmployeeNo(),
                FullName = placeholderName,
                Email = email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                RoleID = null,
                FirmID = null,
                IsActive = false,
                IsProfileCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Firm User account ready (pending email verification): {UserId}", user.UserID);



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