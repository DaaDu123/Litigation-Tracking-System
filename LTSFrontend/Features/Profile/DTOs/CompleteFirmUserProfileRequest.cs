using System.ComponentModel.DataAnnotations;
using LTSFrontend.Shared.Validation;

namespace LTSFrontend.Features.Profile.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Profile.Commands.CompleteFirmUserProfile.CompleteFirmUserProfileCommand</summary>
    public class CompleteFirmUserProfileRequest
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

        public string? ProfileImageUrl { get; set; }
    }
}
