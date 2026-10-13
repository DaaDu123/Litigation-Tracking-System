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


    private int MaxFailedAttempts => _configuration.GetValue("AccountLockout:MaxFailedAttempts", 5);
    private int LockoutDurationMinutes => _configuration.GetValue("AccountLockout:LockoutDurationMinutes", 10);

    private static readonly string DummyPasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy-password-for-timing-normalization");


















    public async Task<LoginResponseDTO> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        _logger.LogInformation("Login attempt for email: {Email}", email);


        var user = await _context.Users.Include(x => x.Role).Include(x => x.Firm).FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user == null)
        {


            _passwordService.VerifyPassword(request.Password, DummyPasswordHash);
            _logger.LogWarning("Login failed: User not found for email: {Email}", email);
            throw new UnauthorizedException("Invalid credentials.");
        }





        if (user.LockoutEndUtc.HasValue)
        {
            if (user.LockoutEndUtc.Value > DateTime.UtcNow)
            {
                var remaining = user.LockoutEndUtc.Value - DateTime.UtcNow;
                _logger.LogWarning("Login blocked: user {UserId} is locked out for {Minutes} more minute(s)", user.UserID, Math.Ceiling(remaining.TotalMinutes));
                throw new UnauthorizedException($"Too many failed login attempts. Please try again in {FormatRemaining(remaining)}.");
            }




            user.LockoutEndUtc = null;
            user.FailedLoginAttempts = 0;
        }


        bool passwordValid = _passwordService.VerifyPassword(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            user.FailedLoginAttempts += 1;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                user.FailedLoginAttempts = 0;
                _logger.LogWarning("User {UserId} locked out for {Minutes} minute(s) after {Count} failed attempts", user.UserID, LockoutDurationMinutes, MaxFailedAttempts);
                await _context.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedException($"Too many failed login attempts. Please try again in {LockoutDurationMinutes} minute{(LockoutDurationMinutes == 1 ? "" : "s")}.");
            }
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Login failed: Invalid password for user: {UserId}", user.UserID);
            throw new UnauthorizedException("Invalid credentials.");
        }


        if (user.IsDeleted)
        {
            _logger.LogWarning("Login failed: Account is deleted for user: {UserId}", user.UserID);
            throw new UnauthorizedException("Account has been deleted.");
        }










        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: Account is inactive (email not verified) for user: {UserId}", user.UserID);
            throw new ValidationException(["Please verify your email address before logging in."]);
        }




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


        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.LastLogin = DateTime.UtcNow;


        var accessToken = _jwtService.GenerateToken(user);
        var accessTokenExpiry = _jwtService.GetAccessTokenExpiry();


        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();





        _context.RefreshTokens.Add(new LTSBackend.Models.Security.RefreshToken
        {
            UserID = user.UserID,
            Token = _jwtService.HashRefreshToken(refreshToken),
            ExpiryDate = refreshTokenExpiry,
            IsRevoked = false
        });


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


        _context.AuditLogs.Add(_auditService.Create(user.UserID, "User Login"));


        await _context.SaveChangesAsync(cancellationToken);


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