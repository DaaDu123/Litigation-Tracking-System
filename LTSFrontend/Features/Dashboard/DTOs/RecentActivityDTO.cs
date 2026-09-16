namespace LTSFrontend.Features.Dashboard.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Dashboard.DTO.cs.RecentActivityDTO</summary>
    public class RecentActivityDTO
    {
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public string? Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
