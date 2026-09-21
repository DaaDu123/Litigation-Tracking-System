using System.ComponentModel.DataAnnotations;
using LTSFrontend.Shared.Validation;

namespace LTSFrontend.Features.Profile.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Profile.Commands.CompleteFirmAdminProfile.CompleteFirmAdminProfileCommand</summary>
    public class CompleteFirmAdminProfileRequest
    {
        [Required(ErrorMessage = "Your full name is required.")]
        [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [PakistaniPhone]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "CNIC is required.")]
        [PakistaniCnic]
        public string CNIC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Firm name is required.")]
        [StringLength(150, ErrorMessage = "Firm name cannot exceed 150 characters.")]
        public string FirmName { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string? Address { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(150, ErrorMessage = "Contact email cannot exceed 150 characters.")]
        public string? ContactEmail { get; set; }

        [PakistaniPhone(AllowEmpty = true)]
        public string? ContactPhone { get; set; }

        public string? ProfileImageUrl { get; set; }
    }
}
