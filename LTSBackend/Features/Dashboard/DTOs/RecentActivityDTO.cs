namespace LTSBackend.Features.Dashboard.DTOs
{
    public class RecentActivityDTO
    {
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public string? Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
