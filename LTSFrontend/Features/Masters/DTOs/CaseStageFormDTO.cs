using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Masters.DTOs
{





    public class CaseStageFormDTO
    {

        public int? StageID { get; set; }

        [Required(ErrorMessage = "Stage name is required.")]
        [StringLength(150, ErrorMessage = "Stage name cannot exceed 150 characters.")]
        public string StageName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEditMode => StageID.HasValue;

        public static CaseStageFormDTO FromDto(CaseStageDTO dto) => new()
        {
            StageID = dto.StageID,
            StageName = dto.StageName,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
    }
}
