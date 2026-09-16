using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Auth.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Auth.ResendOtp.ResendOtpCommand / ResendOtpValidator</summary>
    public class ResendOtpRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
    }
}
