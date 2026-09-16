using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>
    /// Client-side form model for creating/updating a Department. Maps 1:1
    /// onto LTSBackend's CreateDepartmentCommand / UpdateDepartmentCommand,
    /// with validation mirroring CreateDepartmentValidator.
    /// </summary>
    public class DepartmentFormDTO
    {
        /// <summary>Set only when editing an existing department; null on create.</summary>
        public int? DepartmentID { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Department code cannot exceed 20 characters.")]
        public string? DepartmentCode { get; set; }

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEditMode => DepartmentID.HasValue;

        public static DepartmentFormDTO FromDto(DepartmentDTO dto) => new()
        {
            DepartmentID = dto.DepartmentID,
            DepartmentName = dto.DepartmentName,
            DepartmentCode = dto.DepartmentCode,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
    }
}
