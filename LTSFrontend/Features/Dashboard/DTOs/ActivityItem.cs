namespace LTSFrontend.Features.Dashboard.DTOs
{

    public class ActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public DateTime? Timestamp { get; set; }

        public string? Icon { get; set; }

        public string? Href { get; set; }
    }
}
