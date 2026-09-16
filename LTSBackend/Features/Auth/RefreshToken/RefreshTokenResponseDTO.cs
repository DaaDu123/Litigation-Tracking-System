namespace LTSBackend.Features.Auth.RefreshToken;

public class RefreshTokenResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiry { get; set; }
}