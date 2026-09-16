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
    // HANDLE — Anonymous self-registration
    // Validates the email is unused and the firm code is a real,
    // active/unblocked firm, creates the account as InternParalegal
    // (the default lowest-privilege role) with IsActive = false until the
    // email is verified, then generates and emails a 6-digit Registration
    // OTP.
    // =====================================================
    public async Task<RegisterResponseDTO> Handle(RegisterCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting registration for email: {Email}", request.Email);

        // Trim leading/trailing whitespace on all free-text fields so an
        // accidental leading/trailing space typed by the user is never
        // persisted (e.g. " John " -> "John", " user@mail.com " -> "user@mail.com").
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? request.Phone : request.Phone.Trim();
        var department = string.IsNullOrWhiteSpace(request.Department) ? request.Department : request.Department.Trim();

        // 1. Check if email already exists
        bool emailExists = await _context.Users.AsNoTracking().AnyAsync(x => x.Email == email, cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning("Registration failed: Email already exists: {Email}", email);
            throw new ValidationException(new List<string> { "Email already exists." });
        }

        // 2. Resolve the firm this user is registering into
        var firm = await _context.Firms.FirstOrDefaultAsync(x => x.FirmCode == request.FirmCode.Trim().ToUpper(), cancellationToken);

        if (firm == null)
        {
            throw new ValidationException(["Invalid firm code. Please obtain the correct code from your Firm Administrator."]);
        }

        if (firm.IsDeleted)
        {
            throw new ValidationException(["This firm workspace is no longer active."]);
        }

        if (firm.IsBlocked)
        {
            throw new ValidationException(["This firm workspace is currently blocked. Please contact your Firm Administrator."]);
        }

        // 3. Get default role (InternParalegal)
        var defaultRole = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.RoleID == (int)UserRole.InternParalegal, cancellationToken);

        if (defaultRole == null)
        {
            _logger.LogError("Default InternParalegal role not found in database");
            throw new NotFoundException("Default role not found. Please contact administrator.");
        }

        // 3. Create user account
        var user = new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Phone = phone,
            Department = department,
            RoleID = defaultRole.RoleID,
            FirmID = firm.FirmID,
            IsActive = false,  // Inactive until email verified
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            EmployeeNo = GenerateEmployeeNo()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created successfully with ID: {UserId}", user.UserID);

        // 4. Clean up old unused Registration OTPs
        var oldOtps = await _context.UserOtps.Where(x => x.Email == email && !x.IsUsed && x.Purpose == OtpPurpose.Registration).ToListAsync(cancellationToken);

        if (oldOtps.Count > 0)
        {
            _context.UserOtps.RemoveRange(oldOtps);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Removed {Count} old OTPs for {Email}", oldOtps.Count, email);
        }

        // 5. Generate 6-digit OTP
        string otpCode = GenerateSecureOtp();
        _logger.LogInformation("OTP generated for {Email}", email);

        // 6. Save OTP with expiry (Purpose = Registration)
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
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("OTP saved for user: {UserId}", user.UserID);

        // 7. Create audit log
        var auditLog = _auditService.Create(user.UserID, "User Registered");
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        // 8. Send OTP email
        try
        {
            _logger.LogInformation("Sending OTP email to: {Email}", email);
            await _emailService.SendOtpEmailAsync(email, fullName, otpCode);
            _logger.LogInformation("OTP email sent successfully to: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to: {Email}", email);
            // Don't throw — user can use ResendOtp endpoint if email fails
        }

        return new RegisterResponseDTO
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Message = "Registration successful! Please check your email (including Spam/Junk folder) for the OTP code to verify your account."
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