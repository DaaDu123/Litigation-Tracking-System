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







    public async Task<VerifyOtpResponseDTO> Handle(VerifyOtpCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("OTP verification attempt for email: {Email}", request.Email);


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


        var user = await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogError("OTP verification failed: User not found for email: {Email}", request.Email);
            throw new NotFoundException("User not found.");
        }


        user.IsActive = true;
        otp.IsUsed = true;


        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();







        _context.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserID = user.UserID,
            Token = _jwtService.HashRefreshToken(refreshToken),
            ExpiryDate = _jwtService.GetRefreshTokenExpiry(),
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





        _context.AuditLogs.Add(_auditService.Create(user.UserID, "Email Verified via OTP"));


        await _context.SaveChangesAsync(cancellationToken);


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