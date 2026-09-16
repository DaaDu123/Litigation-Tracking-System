namespace LTSFrontend.Features.Dashboard.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Dashboard.DTOs.DashboardDTO</summary>
    public class DashboardDTO
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermissions { get; set; }
        public int TotalAuditLogs { get; set; }
        public int TotalRefreshTokens { get; set; }

        // SuperAdmin-only figures (0 for firm-scoped users)
        public int TotalFirms { get; set; }
        public int ActiveFirms { get; set; }
        public int BlockedFirms { get; set; }

        public List<RecentActivityDTO> RecentActivities { get; set; } = new();
    }
}
