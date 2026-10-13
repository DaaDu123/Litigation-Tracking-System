using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Auth.DTOs
{







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
