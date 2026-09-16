namespace LTSFrontend.Features.AuditLogs.DTOs
{
    public class AuditLogDTO
    {
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Action { get; set; }
        public string? IPAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
