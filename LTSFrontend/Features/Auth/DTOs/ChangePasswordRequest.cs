using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Auth.DTOs
{
    /// <summary>
    /// Mirrors LTSBackend.Features.Auth.ChangePassword.ChangePasswordCommand /
    /// ChangePasswordValidator. Used by the authenticated "Change Password"
    /// screen (POST /api/auth/change-password) - distinct from
    /// ForgotPasswordRequest/ResetPasswordRequest, which are for a user who
    /// is signed OUT and has lost their password entirely.
    /// </summary>
    public class ChangePasswordRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Your current password is required.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A new password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":{}|<>_\-+=\[\]\\/;'~`]).+$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a digit, and a symbol.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Client-side mirror of the backend's NotEqual(OldPassword) rule,
        /// plus the confirm-password match check. Both are re-validated
        /// server-side regardless - this only gives the user instant
        /// feedback instead of a round trip.
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmNewPassword)
            {
                yield return new ValidationResult("New password and confirmation do not match.",new[] { nameof(ConfirmNewPassword) });
            }

            if (!string.IsNullOrEmpty(OldPassword) && !string.IsNullOrEmpty(NewPassword) &&OldPassword == NewPassword)
            {
                yield return new ValidationResult("New password must be different from your current password.",new[] { nameof(NewPassword) });
            }
        }
    }
}
