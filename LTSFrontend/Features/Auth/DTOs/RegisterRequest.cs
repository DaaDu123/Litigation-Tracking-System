using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Auth.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Auth.Register.RegisterCommand / RegisterValidator</summary>
    public class RegisterRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        public string FullName { get; set; } = string.Empty;

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

        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        [RegularExpression(@"^\+?[0-9\-\(\)\s]*$", ErrorMessage = "Phone format is invalid.")]
        public string? Phone { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Firm code is required - ask your Firm Admin for your firm's registration code.")]
        public string FirmCode { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Password) && Password != ConfirmPassword)
            {
                yield return new ValidationResult("Passwords do not match.", new[] { nameof(ConfirmPassword) });
            }
        }
    }
}
