namespace LTSFrontend.Features.Dashboard.DTOs
{

    public class DashboardDTO
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermissions { get; set; }
        public int TotalAuditLogs { get; set; }
        public int TotalRefreshTokens { get; set; }


        public int TotalFirms { get; set; }
        public int ActiveFirms { get; set; }
        public int BlockedFirms { get; set; }

        public List<RecentActivityDTO> RecentActivities { get; set; } = new();
    }
}
