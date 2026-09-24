using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Auth.VerifyOtp;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Jwt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = LTSBackend.Models.Security.RefreshToken;

namespace LTSBackend.Features.Auth.VerifyOtp;

public class VerifyOtpHandler(AppDbContext _context, IJwtService _jwtService, IAuditService _auditService, IHttpContextAccessor _httpContextAccessor, ILogger<VerifyOtpHandler> _logger) : IRequestHandler<VerifyOtpCommand, VerifyOtpResponseDTO>
{

    // =====================================================
    // HANDLE — Anonymous, completes the registration flow
    // Validates the emailed Registration OTP, activates the account
    // (IsActive = true), issues an access + refresh token pair so the
    // user is immediately logged in, and writes an audit log entry.
    // =====================================================
    public async Task<VerifyOtpResponseDTO> Handle(VerifyOtpCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("OTP verification attempt for email: {Email}", request.Email);

        // 1. Find and validate OTP (Purpose must be Registration)
        var otp = await _context.UserOtps
            .FirstOrDefaultAsync(x =>
                x.Email == request.Email &&
                x.OtpCode == request.OtpCode &&
                x.Purpose == OtpPurpose.Registration &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (otp == null)
        {
            _logger.LogWarning("OTP verification failed: Invalid or expired OTP for email: {Email}", request.Email);
            throw new ValidationException(new List<string> { "Invalid or expired OTP code." });
        }

        // 2. Find user and load role
        var user = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogError("OTP verification failed: User not found for email: {Email}", request.Email);
            throw new NotFoundException("User not found.");
        }

        // 3. Activate user account and mark OTP as used
        user.IsActive = true;
        otp.IsUsed = true;

        // 4. Generate tokens
        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // 5. Save refresh token
        // FIX: expiry was hardcoded to 7 days here, inconsistent
        // with Login/RefreshToken handlers which use the configured
        // JwtSettings.RefreshTokenDays via GetRefreshTokenExpiry().
        // SECURITY: only the SHA-256 hash is persisted, matching
        // LoginHandler/RefreshTokenHandler — never the raw token.
        _context.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserID = user.UserID,
            Token = _jwtService.HashRefreshToken(refreshToken),
            ExpiryDate = _jwtService.GetRefreshTokenExpiry(),
            IsRevoked = false
        });

        // 6. Record login in LoginHistory
        // FIX: this handler logs the user in (issues an access + refresh
        // token pair) exactly like LoginHandler does, but never wrote a
        // LoginHistory row for it. That meant anyone who verified via OTP
        // had NO open session row at all - so when they eventually hit
        // Logout, LogoutHandler's "most recent open row for this user"
        // lookup either found nothing to close, or (worse) latched onto a
        // stale/unrelated older row and closed that one instead, which is
        // one of the reasons LogoutTime looked like it wasn't being saved.
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

        // 7. Create audit log
        // FIX: every other significant auth action (Login, Logout,
        // Register, ResetPassword, ChangePassword, RefreshToken)
        // writes an audit entry — this was missing here.
        _context.AuditLogs.Add(_auditService.Create(user.UserID, "Email Verified via OTP"));

        // 8. Save all changes
        await _context.SaveChangesAsync(cancellationToken);

        // 9. Set refresh token cookie
        if (_httpContextAccessor.HttpContext != null)
        {
            _jwtService.SetRefreshTokenCookie(_httpContextAccessor.HttpContext.Response, refreshToken);
        }

        _logger.LogInformation("OTP verified successfully for user: {UserId}", user.UserID);

        return new VerifyOtpResponseDTO
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role?.RoleName,
            AccessToken = accessToken,
            AccessTokenExpiry = _jwtService.GetAccessTokenExpiry(),
            Message = "Email verified successfully! You can now login."
        };
    }
}