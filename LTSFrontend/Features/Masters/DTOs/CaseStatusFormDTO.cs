using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>
    /// Client-side form model for creating/updating a Case Status. Maps 1:1
    /// onto LTSBackend's CreateCaseStatusCommand / UpdateCaseStatusCommand,
    /// with validation mirroring CreateCaseStatusValidator.
    /// </summary>
    public class CaseStatusFormDTO
    {
        /// <summary>Set only when editing an existing status; null on create.</summary>
        public int? StatusID { get; set; }

        [Required(ErrorMessage = "Status name is required.")]
        [StringLength(100, ErrorMessage = "Status name cannot exceed 100 characters.")]
        public string StatusName { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Sequence number must be zero or greater.")]
        public int SequenceNo { get; set; }

        [Required(ErrorMessage = "Color code is required.")]
        [StringLength(10, ErrorMessage = "Color code cannot exceed 10 characters.")]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color code must be a valid hex color, e.g. #FF0000.")]
        public string ColorCode { get; set; } = "#1B4FD6";

        public bool IsClosed { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEditMode => StatusID.HasValue;

        public static CaseStatusFormDTO FromDto(CaseStatusDTO dto) => new()
        {
            StatusID = dto.StatusID,
            StatusName = dto.StatusName,
            SequenceNo = dto.SequenceNo,
            ColorCode = dto.ColorCode,
            IsClosed = dto.IsClosed,
            IsActive = dto.IsActive
        };
    }
}
