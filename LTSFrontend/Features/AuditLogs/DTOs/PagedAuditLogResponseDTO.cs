namespace LTSFrontend.Features.AuditLogs.DTOs
{
    public class PagedAuditLogResponseDTO
    {
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<AuditLogDTO> Records { get; set; } = new();
    }
}
