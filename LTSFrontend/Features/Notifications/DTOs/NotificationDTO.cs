namespace LTSFrontend.Features.Notifications.DTOs
{
    public class NotificationDTO
    {
        public long NotificationID { get; set; }
        public int NotificationTypeID { get; set; }
        public string NotificationTypeName { get; set; } = string.Empty;
        public long? CaseID { get; set; }
        public string? CaseTitle { get; set; }
        public string? CaseNumber { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public string Priority { get; set; } = "Medium";
        public DateTime CreatedDate { get; set; }
    }
}
