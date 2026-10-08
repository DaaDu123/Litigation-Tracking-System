namespace LTSBackend.Services.Jwt;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;

    /// <summary>When > 0, the refresh-token lifetime in MINUTES (overrides RefreshTokenDays). 0 = use RefreshTokenDays.</summary>
    public int RefreshTokenMinutes { get; set; } = 0;
    public bool UseSecureCookies { get; set; } = true;
}