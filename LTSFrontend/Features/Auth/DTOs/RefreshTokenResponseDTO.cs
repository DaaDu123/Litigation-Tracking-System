namespace LTSFrontend.Features.Auth.DTOs
{

    public class RefreshTokenResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }
    }
}
