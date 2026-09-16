using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>
    /// Client-side form model for creating/updating a Court. Maps 1:1 onto
    /// LTSBackend's CreateCourtCommand / UpdateCourtCommand records, with
    /// validation rules mirroring CreateCourtValidator / UpdateCourtValidator
    /// so the person gets instant feedback before the round-trip to the API.
    /// </summary>
    public class CourtFormDTO
    {
        /// <summary>Set only when editing an existing court; null on create.</summary>
        public int? CourtID { get; set; }

        [Required(ErrorMessage = "Court name is required.")]
        [StringLength(150, ErrorMessage = "Court name cannot exceed 150 characters.")]
        public string CourtName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Court type cannot exceed 100 characters.")]
        public string? CourtType { get; set; }

        [StringLength(200, ErrorMessage = "Jurisdiction cannot exceed 200 characters.")]
        public string? Jurisdiction { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEditMode => CourtID.HasValue;

        public static CourtFormDTO FromDto(CourtDTO dto) => new()
        {
            CourtID = dto.CourtID,
            CourtName = dto.CourtName,
            CourtType = dto.CourtType,
            Jurisdiction = dto.Jurisdiction,
            Address = dto.Address,
            IsActive = dto.IsActive
        };
    }
}
