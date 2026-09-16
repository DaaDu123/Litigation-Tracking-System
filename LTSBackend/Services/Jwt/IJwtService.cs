using LTSBackend.Models.Security;
using Microsoft.AspNetCore.Http;
namespace LTSBackend.Services.Jwt;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();

    string HashRefreshToken(string rawToken);

    DateTime GetAccessTokenExpiry();
    DateTime GetRefreshTokenExpiry();
    void SetRefreshTokenCookie(HttpResponse response, string refreshToken);
    void RemoveRefreshTokenCookie(HttpResponse response);
}