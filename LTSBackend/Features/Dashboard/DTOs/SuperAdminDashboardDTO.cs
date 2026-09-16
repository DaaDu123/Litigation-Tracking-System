namespace LTSBackend.Features.Dashboard.DTOs;

public class SuperAdminDashboardDTO
{
    public int TotalFirms { get; set; }
    public int ActiveFirms { get; set; }
    public int BlockedFirms { get; set; }
    public int PendingDomainMigrations { get; set; }

    public int TotalFirmAdmins { get; set; }

    public int TotalSystemAuditLogs { get; set; }
    public int TotalActiveSessions { get; set; }

    public List<RecentActivityDTO> RecentActivities { get; set; } = [];
}
