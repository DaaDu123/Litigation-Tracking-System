namespace LTSFrontend.Features.LoginHistory.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.LoginHistory.DTOs.LoginHistoryFilterDTO</summary>
    public class LoginHistoryFilterDTO
    {
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
