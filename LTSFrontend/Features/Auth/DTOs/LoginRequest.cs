using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Auth.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Auth.Login.LoginCommand / LoginValidator</summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
