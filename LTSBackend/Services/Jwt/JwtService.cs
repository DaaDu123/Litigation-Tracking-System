using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LTSBackend.Models.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace LTSBackend.Services.Jwt;

public class JwtService(IConfiguration _configuration, ILogger<JwtService> _logger) : IJwtService
{
    // Issues a short-lived signed access token carrying identity, tenant (FirmID),
    // role and security-stamp claims used by every downstream authorization check.
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Email, user.Email),

            // Unique token identifier (JWT ID). Not currently checked against a
            // blacklist, but present so a revocation/blacklist store can be added
            // later without re-issuing the token contract.
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // Security stamp - compared against the current DB value on every
            // request (Program.cs OnTokenValidated). If the user's password is
            // reset, the account is blocked, or an admin forces logout, the stamp
            // is regenerated and this token is rejected immediately even though
            // it has not expired.
            new("SecurityStamp", user.SecurityStamp)
        };

        // FirmID claim - drives multi-tenant row-level scoping on every
        // request (null/absent for SuperAdmin = no firm restriction).
        if (user.FirmID.HasValue)
        {
            claims.Add(new Claim("FirmID", user.FirmID.Value.ToString()));
        }

        // Add role claim
        var role = user.GetRole();
        if (role.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Value.ToString()));
        }
        else if (!string.IsNullOrWhiteSpace(user.Role?.RoleName))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
        }

        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
        var issuer = _configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured.");
        var audience = _configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("JwtSettings:Audience is not configured.");

        if (!int.TryParse(_configuration["JwtSettings:ExpiryMinutes"], out var expiryMinutes))
            expiryMinutes = 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Generates a cryptographically random opaque refresh token (not a JWT) that
    // is stored server-side (RefreshTokens table) and rotated on every use.
    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    // Computes the absolute UTC expiry timestamp for a newly issued access token.
    public DateTime GetAccessTokenExpiry()
    {
        int expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes", 60);
        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    // Computes the absolute UTC expiry timestamp for a newly issued refresh token.
    public DateTime GetRefreshTokenExpiry()
    {
        int refreshTokenDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenDays", 7);
        return DateTime.UtcNow.AddDays(refreshTokenDays);
    }

    public string HashRefreshToken(string rawToken)
    {
        if (string.IsNullOrEmpty(rawToken))
        {
            throw new ArgumentException("Refresh token cannot be null or empty.", nameof(rawToken));
        }

        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
    // Sets the HttpOnly refresh-token cookie on the response, using the same
    // JwtSettings configuration values (UseSecureCookies, RefreshTokenDays)
    // already read by GetRefreshTokenExpiry() above.
    public void SetRefreshTokenCookie(HttpResponse response, string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("Attempted to set null or empty refresh token");
            return;
        }

        bool useSecureCookies = _configuration.GetValue<bool>("JwtSettings:UseSecureCookies", true);
        int refreshTokenDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenDays", 7);

        response.Cookies.Append("refreshToken",refreshToken,new CookieOptions
            {
                HttpOnly = true,
                Secure = useSecureCookies,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Expires = DateTime.UtcNow.AddDays(refreshTokenDays)
            });

        _logger.LogDebug("Refresh token cookie set with {Days} days expiry", refreshTokenDays);
    }

    // Removes the refresh-token cookie from the response (used on logout).
    public void RemoveRefreshTokenCookie(HttpResponse response)
    {
        bool useSecureCookies = _configuration.GetValue<bool>("JwtSettings:UseSecureCookies", true);
        response.Cookies.Delete("refreshToken",new CookieOptions
            {
                HttpOnly = true,
                Secure = useSecureCookies,
                SameSite = SameSiteMode.Strict
            });

        _logger.LogDebug("Refresh token cookie removed");
    }
}