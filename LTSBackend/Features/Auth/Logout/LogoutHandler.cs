using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Auth.Logout;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Jwt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Auth.Logout;

public class LogoutHandler(AppDbContext _context, IHttpContextAccessor _httpContextAccessor, IAuditService _auditService, IJwtService _jwtService, ILogger<LogoutHandler> _logger) : IRequestHandler<LogoutCommand, bool>
{






    public async Task<bool> Handle(LogoutCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout attempt");


        var refreshToken = _httpContextAccessor.HttpContext? .Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("Logout failed: No refresh token in cookie");
            throw new UnauthorizedException("Refresh token not found.");
        }





        var tokenHash = _jwtService.HashRefreshToken(refreshToken);

        var token = await _context.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == tokenHash,cancellationToken);

        if (token == null)
        {
            _logger.LogWarning("Logout failed: Refresh token not found in database");
            throw new UnauthorizedException("Invalid refresh token.");
        }


        token.IsRevoked = true;
        _logger.LogInformation("Refresh token revoked for user: {UserId}", token.UserID);


        var loginHistory = await _context.LoginHistories
            .Where(x => x.UserID == token.UserID && !x.IsLoggedOut)
            .OrderByDescending(x => x.LoginTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (loginHistory != null)
        {
            loginHistory.IsLoggedOut = true;
            loginHistory.LogoutTime = DateTime.UtcNow;
            loginHistory.Status = "Logout";
            _logger.LogInformation("Login history updated for user: {UserId}", token.UserID);
        }


        _context.AuditLogs.Add(_auditService.Create(token.UserID, "User Logout"));


        await _context.SaveChangesAsync(cancellationToken);


        _jwtService.RemoveRefreshTokenCookie(_httpContextAccessor.HttpContext!.Response);

        _logger.LogInformation("User {UserId} logged out successfully", token.UserID);

        return true;
    }
}