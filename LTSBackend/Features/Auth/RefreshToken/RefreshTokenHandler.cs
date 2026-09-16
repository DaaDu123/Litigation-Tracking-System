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
    // =====================================================
    // HANDLE — Anonymous (relies on the refresh-token cookie)
    // Validates the stored (hashed) refresh token — rejecting it if
    // revoked, expired, or its user is inactive/deleted — then rotates
    // it (old one revoked, new one issued) and returns a fresh access
    // token. SECURITY: reuse of an already-revoked token is treated as
    // a possible theft signal and revokes ALL of that user's active
    // sessions.
    // =====================================================
    public async Task<RefreshTokenResponseDTO> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh attempt");

        // 1. Read refresh token from cookie
        var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("Token refresh failed: No refresh token in cookie");
            throw new UnauthorizedException("Refresh token not found.");
        }

        // 2. Find stored refresh token with user and role
        // SECURITY: refresh tokens are stored as a SHA-256 hash, never
        // the raw value (see IJwtService.HashRefreshToken) — hash the
        // incoming cookie value before looking it up.
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

        // 3. Validate token state
        if (storedToken.IsRevoked)
        {
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

        // 4. Validate user
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

        // 5. Revoke old refresh token (Token Rotation)
        storedToken.IsRevoked = true;

        // 6. Generate new access token
        var accessToken = _jwtService.GenerateToken(user);
        var accessTokenExpiry = _jwtService.GetAccessTokenExpiry();

        // 7. Generate new refresh token
        // SECURITY: store only the hash, matching Login/VerifyOtp — the
        // raw value is set on the cookie in step 10 and never persisted.
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();

        _context.RefreshTokens.Add(new LTSBackend.Models.Security.RefreshToken
        {
            UserID = user.UserID,
            Token = _jwtService.HashRefreshToken(newRefreshToken),
            ExpiryDate = newRefreshTokenExpiry,
            IsRevoked = false
        });

        // 8. Create audit log
        _context.AuditLogs.Add(_auditService.Create(user.UserID, "Token Refreshed"));

        // 9. Save all changes
        await _context.SaveChangesAsync(cancellationToken);

        // 10. Update refresh token cookie (raw value — never the hash)
        _jwtService.SetRefreshTokenCookie(_httpContextAccessor.HttpContext!.Response,newRefreshToken);

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.UserID);

        return new RefreshTokenResponseDTO
        {
            AccessToken = accessToken,
            AccessTokenExpiry = accessTokenExpiry
        };
    }
}
