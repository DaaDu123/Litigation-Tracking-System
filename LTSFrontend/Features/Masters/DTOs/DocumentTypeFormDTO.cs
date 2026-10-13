using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Masters.DTOs
{





    public class DocumentTypeFormDTO
    {

        public int? DocumentTypeID { get; set; }

        [Required(ErrorMessage = "Type name is required.")]
        [StringLength(160, ErrorMessage = "Type name cannot exceed 160 characters.")]
        public string TypeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEditMode => DocumentTypeID.HasValue;

        public static DocumentTypeFormDTO FromDto(DocumentTypeDTO dto) => new()
        {
            DocumentTypeID = dto.DocumentTypeID,
            TypeName = dto.TypeName,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
    }
}
