using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Masters.DTOs
{





    public class CaseCategoryFormDTO
    {

        public int? CategoryID { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(150, ErrorMessage = "Category name cannot exceed 150 characters.")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEditMode => CategoryID.HasValue;

        public static CaseCategoryFormDTO FromDto(CaseCategoryDTO dto) => new()
        {
            CategoryID = dto.CategoryID,
            CategoryName = dto.CategoryName,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
    }
}
