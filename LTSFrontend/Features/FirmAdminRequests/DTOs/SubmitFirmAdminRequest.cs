using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.FirmAdminRequests.DTOs
{
    /// <summary>
    /// Mirrors LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequest.SubmitFirmAdminRequestCommand -
    /// Email + Password only now. Firm name/code and personal details are
    /// collected later, during mandatory profile completion after
    /// SuperAdmin approval.
    /// </summary>
    public class SubmitFirmAdminRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":{}|<>_\-+=\[\]\\/;'~`]).+$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a digit, and a symbol.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Password) && Password != ConfirmPassword)
            {
                yield return new ValidationResult("Passwords do not match.", new[] { nameof(ConfirmPassword) });
            }
        }
    }
}
