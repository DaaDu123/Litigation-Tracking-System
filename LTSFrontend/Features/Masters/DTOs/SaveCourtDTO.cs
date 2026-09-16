using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>
    /// Create/Update payload for a Court. CourtID is null on create.
    /// Mirrors CreateCourtCommand / UpdateCourtCommand on the backend.
    /// </summary>
    public class SaveCourtDTO
    {
        public int? CourtID { get; set; }

        [Required(ErrorMessage = "Court name is required.")]
        [MaxLength(150, ErrorMessage = "Court name cannot exceed 150 characters.")]
        public string CourtName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Court type cannot exceed 100 characters.")]
        public string? CourtType { get; set; }

        [MaxLength(200, ErrorMessage = "Jurisdiction cannot exceed 200 characters.")]
        public string? Jurisdiction { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
