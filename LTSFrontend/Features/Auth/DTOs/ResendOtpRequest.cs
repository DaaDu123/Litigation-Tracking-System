using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Auth.DTOs
{

    public class ResendOtpRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
    }
}
