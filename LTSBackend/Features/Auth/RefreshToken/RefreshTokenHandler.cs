using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Auth.RefreshToken;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Jwt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Auth.RefreshToken;

public class RefreshTokenHandler(AppDbContext _context, IJwtService _jwtService, IHttpContextAccessor _httpContextAccessor, IAuditService _auditService, ILogger<RefreshTokenHandler> _logger) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDTO>
{









    public async Task<RefreshTokenResponseDTO> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh attempt");


        var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("Token refresh failed: No refresh token in cookie");
            throw new UnauthorizedException("Refresh token not found.");
        }





        var tokenHash = _jwtService.HashRefreshToken(refreshToken);

        var storedToken = await _context.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Token == tokenHash,cancellationToken);

        if (storedToken == null)
        {
            _logger.LogWarning("Token refresh failed: Refresh token not found in database");
            throw new UnauthorizedException("Invalid refresh token.");
        }


        if (storedToken.IsRevoked)
        {
            var recentReplacement = await _context.RefreshTokens
                .Where(x => x.UserID == storedToken.UserID
                            && !x.IsRevoked
                            && x.ExpiryDate > DateTime.UtcNow
                            && x.CreatedAt >= DateTime.UtcNow.AddSeconds(-15))
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.User).ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(cancellationToken);

            if (recentReplacement != null)
            {
                _logger.LogInformation(
                    "Token refresh: reused an already-rotated token for user {UserId} within the grace window - treating as a benign concurrent-circuit race, not theft.",
                    storedToken.UserID);

                var graceAccessToken = _jwtService.GenerateToken(recentReplacement.User);
                var graceAccessTokenExpiry = _jwtService.GetAccessTokenExpiry();
                var graceRefreshToken = _jwtService.GenerateRefreshToken();
                var graceRefreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

                _context.RefreshTokens.Add(new LTSBackend.Models.Security.RefreshToken
                {
                    UserID = storedToken.UserID,
                    Token = _jwtService.HashRefreshToken(graceRefreshToken),
                    ExpiryDate = graceRefreshTokenExpiry,
                    IsRevoked = false
                });
                await _context.SaveChangesAsync(cancellationToken);

                _jwtService.SetRefreshTokenCookie(_httpContextAccessor.HttpContext!.Response, graceRefreshToken);

                return new RefreshTokenResponseDTO
                {
                    AccessToken = graceAccessToken,
                    AccessTokenExpiry = graceAccessTokenExpiry
                };
            }

            var otherActiveTokens = await _context.RefreshTokens.Where(x => x.UserID == storedToken.UserID && !x.IsRevoked).ToListAsync(cancellationToken);

            if (otherActiveTokens.Count > 0)
            {
                foreach (var t in otherActiveTokens)
                {
                    t.IsRevoked = true;
                }

                _context.AuditLogs.Add(_auditService.Create(storedToken.UserID,"SECURITY ALERT: Revoked refresh token reuse detected — all active sessions revoked"));

                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogWarning("Token refresh failed: Reuse of a revoked token detected for user {UserId}. " + "All {Count} active session(s) for this user have been revoked.",storedToken.UserID,otherActiveTokens.Count);

            throw new UnauthorizedException("This session is no longer valid. For your security, all sessions have been logged out — please log in again.");
        }

        if (storedToken.ExpiryDate <= DateTime.UtcNow)
        {
            _logger.LogWarning("Token refresh failed: Token expired for user: {UserId}", storedToken.UserID);
            throw new UnauthorizedException("Refresh token has expired.");
        }

        var user = storedToken.User;


        if (user == null)
        {
            _logger.LogError("Token refresh failed: User not found");
            throw new UnauthorizedException("User not found.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Token refresh failed: User account inactive: {UserId}", user.UserID);
            throw new UnauthorizedException("User account is inactive.");
        }

        if (user.IsDeleted)
        {
            _logger.LogWarning("Token refresh failed: User account deleted: {UserId}", user.UserID);
            throw new UnauthorizedException("User account has been deleted.");
        }


        storedToken.IsRevoked = true;


        var accessToken = _jwtService.GenerateToken(user);
        var accessTokenExpiry = _jwtService.GetAccessTokenExpiry();




        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        _context.RefreshTokens.Add(new LTSBackend.Models.Security.RefreshToken
        {
            UserID = user.UserID,
            Token = _jwtService.HashRefreshToken(newRefreshToken),
            ExpiryDate = newRefreshTokenExpiry,
            IsRevoked = false
        });


        _context.AuditLogs.Add(_auditService.Create(user.UserID, "Token Refreshed"));


        await _context.SaveChangesAsync(cancellationToken);


        _jwtService.SetRefreshTokenCookie(_httpContextAccessor.HttpContext!.Response,newRefreshToken);

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.UserID);

        return new RefreshTokenResponseDTO
        {
            AccessToken = accessToken,
            AccessTokenExpiry = accessTokenExpiry
        };
    }
}
