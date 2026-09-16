using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Auth.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Auth.ResetPassword.ResetPasswordCommand / ResetPasswordValidator.
    /// Email + OtpCode come from the forgot-password step; both are sent
    /// together with the new password in one call (see ResetPasswordHandler).</summary>
    public class ResetPasswordRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP code is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP code must be 6 digits.")]
        public string OtpCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":{}|<>_\-+=\[\]\\/;'~`]).+$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a digit, and a symbol.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmPassword)
            {
                yield return new ValidationResult(
                    "Passwords do not match.",
                    new[] { nameof(ConfirmPassword) });
            }
        }
    }
}
