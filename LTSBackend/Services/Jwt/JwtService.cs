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


    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Email, user.Email),




            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),






            new("SecurityStamp", user.SecurityStamp)
        };



        if (user.FirmID.HasValue)
        {
            claims.Add(new Claim("FirmID", user.FirmID.Value.ToString()));
        }


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



    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }


    public DateTime GetAccessTokenExpiry()
    {
        int expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes", 60);
        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }


    public DateTime GetRefreshTokenExpiry()
    {
        return DateTime.UtcNow.Add(GetRefreshTokenLifetime());
    }





    private TimeSpan GetRefreshTokenLifetime()
    {
        int minutes = _configuration.GetValue<int>("JwtSettings:RefreshTokenMinutes", 0);
        if (minutes > 0)
            return TimeSpan.FromMinutes(minutes);

        int days = _configuration.GetValue<int>("JwtSettings:RefreshTokenDays", 7);
        return TimeSpan.FromDays(days);
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



    public void SetRefreshTokenCookie(HttpResponse response, string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("Attempted to set null or empty refresh token");
            return;
        }

        bool useSecureCookies = _configuration.GetValue<bool>("JwtSettings:UseSecureCookies", true);
        var refreshLifetime = GetRefreshTokenLifetime();

        response.Cookies.Append("refreshToken",refreshToken,new CookieOptions
            {
                HttpOnly = true,
                Secure = useSecureCookies,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Expires = DateTime.UtcNow.Add(refreshLifetime)
            });

        _logger.LogDebug("Refresh token cookie set with {Minutes} minutes expiry", refreshLifetime.TotalMinutes);
    }


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