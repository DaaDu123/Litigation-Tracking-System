using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Jwt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Auth.Login;

public class LoginHandler(AppDbContext _context, IPasswordService _passwordService, IJwtService _jwtService, IAuditService _auditService, IHttpContextAccessor _httpContextAccessor, IConfiguration _configuration, ILogger<LoginHandler> _logger) : IRequestHandler<LoginCommand, LoginResponseDTO>
{
    // RATE LIMITING / ACCOUNT LOCKOUT (configurable via AccountLockout in
    // appsettings.json; defaults below apply only if config is missing).
    private int MaxFailedAttempts => _configuration.GetValue("AccountLockout:MaxFailedAttempts", 5);
    private int LockoutDurationMinutes => _configuration.GetValue("AccountLockout:LockoutDurationMinutes", 10);

    private static readonly string DummyPasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy-password-for-timing-normalization");

    // =====================================================
    // HANDLE — Anonymous login attempt
    // Validates email/password with account-lockout protection (locks the
    // account after MaxFailedAttempts, using a dummy password verify on a
    // not-found email so timing doesn't reveal which emails exist),
    // rejects deleted/unverified accounts and blocked/removed firm
    // workspaces, then issues an access + refresh token pair, records the
    // login in LoginHistory, and writes an audit log entry.
    // =====================================================
    public async Task<LoginResponseDTO> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        _logger.LogInformation("Login attempt for email: {Email}", email);

        // 1. Find user by email with role
        var user = await _context.Users.Include(x => x.Role).Include(x => x.Firm).FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user == null)
        {
            // Run a dummy verify so this branch costs about the same as the
            // "wrong password" branch below (see DummyPasswordHash comment).
            _passwordService.VerifyPassword(request.Password, DummyPasswordHash);
            _logger.LogWarning("Login failed: User not found for email: {Email}", email);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // 2. Check lockout status BEFORE spending time on a password
        // verify - if still locked, fail fast with a clear remaining-time
        // message. If the lockout window has already passed, clear it and
        // give the user a fresh set of attempts.
        if (user.LockoutEndUtc.HasValue)
        {
            if (user.LockoutEndUtc.Value > DateTime.UtcNow)
            {
                var remaining = user.LockoutEndUtc.Value - DateTime.UtcNow;
                _logger.LogWarning("Login blocked: user {UserId} is locked out for {Minutes} more minute(s)", user.UserID, Math.Ceiling(remaining.TotalMinutes));
                throw new UnauthorizedException($"Too many failed login attempts. Please try again in {FormatRemaining(remaining)}.");
            }

            // Lockout window has passed - clear it and reset the counter
            // so this attempt (and the next MaxFailedAttempts-1 after it,
            // if wrong) are evaluated fresh.
            user.LockoutEndUtc = null;
            user.FailedLoginAttempts = 0;
        }

        // 3. Verify password (+ track failed attempts / auto-lock)
        bool passwordValid = _passwordService.VerifyPassword(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            user.FailedLoginAttempts += 1;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                user.FailedLoginAttempts = 0; // next window starts clean once LockoutEndUtc passes
                _logger.LogWarning("User {UserId} locked out for {Minutes} minute(s) after {Count} failed attempts", user.UserID, LockoutDurationMinutes, MaxFailedAttempts);
                await _context.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedException($"Too many failed login attempts. Please try again in {LockoutDurationMinutes} minute{(LockoutDurationMinutes == 1 ? "" : "s")}.");
            }
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Login failed: Invalid password for user: {UserId}", user.UserID);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // 4. Check if account is deleted
        if (user.IsDeleted)
        {
            _logger.LogWarning("Login failed: Account is deleted for user: {UserId}", user.UserID);
            throw new UnauthorizedException("Account has been deleted.");
        }

        // 5. Check if account is active
        // IsActive here means one thing only now: "email not yet
        // verified" (relevant for any pre-existing accounts still in this
        // state from before the self-registration flow was removed - new
        // users now come in pre-activated via UserJoinRequest approval or
        // Firm/Super Admin creation). Failed-
        // login lockout is handled entirely via LockoutEndUtc above and
        // never touches this field anymore, so this message is now
        // accurate every time it's shown.
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: Account is inactive (email not verified) for user: {UserId}", user.UserID);
            throw new ValidationException(["Please verify your email address before logging in."]);
        }

        // 5b. Check if the user's firm workspace is still usable
        // (multi-tenancy: a blocked/removed firm locks out every
        // user under it, regardless of their own account status)
        if (user.Firm != null)
        {
            if (user.Firm.IsDeleted)
            {
                _logger.LogWarning("Login failed: Firm {FirmID} is removed", user.FirmID);
                throw new UnauthorizedException("This firm workspace is no longer active.");
            }
            if (user.Firm.IsBlocked)
            {
                _logger.LogWarning("Login failed: Firm {FirmID} is blocked", user.FirmID);
                throw new UnauthorizedException("Your firm's workspace is currently blocked. Please contact your administrator.");
            }
        }

        // 6. Reset failed attempts + lockout state + update last login time
        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.LastLogin = DateTime.UtcNow;

        // 7. Generate access token
        var accessToken = _jwtService.GenerateToken(user);
        var accessTokenExpiry = _jwtService.GetAccessTokenExpiry();

        // 8. Generate refresh token
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        // 9. Save refresh token to database
        // SECURITY: only the SHA-256 hash is persisted — never the raw
        // token. The raw value is only ever sent to the client, in the
        // HttpOnly cookie set below.
        _context.RefreshTokens.Add(new LTSBackend.Models.Security.RefreshToken
        {
            UserID = user.UserID,
            Token = _jwtService.HashRefreshToken(refreshToken),
            ExpiryDate = refreshTokenExpiry,
            IsRevoked = false
        });

        // 10. Record login in LoginHistory
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

        _context.LoginHistories.Add(new LTSBackend.Models.Security.LoginHistory
        {
            UserID = user.UserID,
            LoginTime = DateTime.UtcNow,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            Status = "Success",
            IsLoggedOut = false
        });

        // 11. Create audit log
        _context.AuditLogs.Add(_auditService.Create(user.UserID, "User Login"));

        // 12. Save all changes
        await _context.SaveChangesAsync(cancellationToken);

        // 13. Set refresh token in HTTP cookie
        _jwtService.SetRefreshTokenCookie(_httpContextAccessor.HttpContext!.Response, refreshToken);
        _logger.LogInformation("User {UserId} logged in successfully", user.UserID);

        return new LoginResponseDTO
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role?.RoleName,
            AccessToken = accessToken,
            AccessTokenExpiry = accessTokenExpiry
        };
    }

    /// <summary>Human-friendly "try again in ..." text for the lockout message.</summary>
    private static string FormatRemaining(TimeSpan remaining)
    {
        if (remaining.TotalHours >= 1)
        {
            int hours = (int)Math.Ceiling(remaining.TotalHours);
            return $"{hours} hour{(hours == 1 ? "" : "s")}";
        }

        int minutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
        return $"{minutes} minute{(minutes == 1 ? "" : "s")}";
    }
}