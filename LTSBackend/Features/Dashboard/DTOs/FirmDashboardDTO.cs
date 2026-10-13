namespace LTSBackend.Features.Dashboard.DTOs;

















public class FirmDashboardDTO
{
    public string Scope { get; set; } = "AssignedOnly";

    public int TotalCases { get; set; }
    public int ActiveCases { get; set; }
    public int ClosedCases { get; set; }

    public int UpcomingHearings7Days { get; set; }
    public int PendingDeadlines { get; set; }
    public int OverdueDeadlines { get; set; }


    public int? TotalFirmUsers { get; set; }
}
