using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.FirmAdminRequests.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequest.SubmitFirmAdminRequestCommand</summary>
    public class SubmitFirmAdminRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Firm name is required.")]
        [StringLength(150, ErrorMessage = "Firm name cannot exceed 150 characters.")]
        public string FirmName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Firm code is required.")]
        [StringLength(30, ErrorMessage = "Firm code cannot exceed 30 characters.")]
        [RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "Firm code can only contain letters, numbers, and hyphens.")]
        public string FirmCode { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string? Address { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(150, ErrorMessage = "Contact email cannot exceed 150 characters.")]
        public string? ContactEmail { get; set; }

        [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters.")]
        public string? ContactPhone { get; set; }

        [Required(ErrorMessage = "Your full name is required.")]
        [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        public string AdminFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Your email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string AdminEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":{}|<>_\-+=\[\]\\/;'~`]).+$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a digit, and a symbol.")]
        public string AdminPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        [RegularExpression(@"^\+?[0-9\-\(\)\s]*$", ErrorMessage = "Phone format is invalid.")]
        public string? AdminPhone { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(AdminPassword) && AdminPassword != ConfirmPassword)
            {
                yield return new ValidationResult("Passwords do not match.", new[] { nameof(ConfirmPassword) });
            }
        }
    }
}
