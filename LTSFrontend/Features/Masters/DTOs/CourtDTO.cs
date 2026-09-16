namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Courts.DTOs.CourtDTO</summary>
    public class CourtDTO
    {
        public int CourtID { get; set; }
        public string CourtName { get; set; } = string.Empty;
        public string? CourtType { get; set; }
        public string? Jurisdiction { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }

        /// <summary>True = system-wide record shared across all firms. Only SuperAdmin can edit/deactivate/delete these.</summary>
        public bool IsGlobal { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
