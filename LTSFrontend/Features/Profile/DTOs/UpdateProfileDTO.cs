using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Profile.DTOs
{
    /// <summary>Client-side form model for "My Profile". Mirrors LTSBackend's
    /// UpdateMyProfileCommand / UpdateMyProfileValidator.</summary>
    public class UpdateProfileDTO
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        [RegularExpression(@"^\+?[0-9\-\(\)\s]*$", ErrorMessage = "Phone format is invalid.")]
        public string? Phone { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
        public string? Department { get; set; }

        public static UpdateProfileDTO FromDto(ProfileDTO dto) => new()
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Department = dto.Department
        };
    }
}
