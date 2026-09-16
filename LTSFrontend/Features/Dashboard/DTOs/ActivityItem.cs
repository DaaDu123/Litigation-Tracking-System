namespace LTSFrontend.Features.Dashboard.DTOs
{
    /// <summary>One row in a <c>RecentActivityPanel</c> (an audit log entry, a recently touched case, etc.).</summary>
    public class ActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public DateTime? Timestamp { get; set; }
        /// <summary>Bootstrap Icons class, e.g. "bi bi-briefcase". Defaults to a generic dot if omitted.</summary>
        public string? Icon { get; set; }
        /// <summary>Optional click-through link (case details, user profile, etc.).</summary>
        public string? Href { get; set; }
    }
}
