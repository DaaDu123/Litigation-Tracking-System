namespace LTSFrontend.Features.Dashboard.DTOs
{
    /// <summary>
    /// Mirrors LTSBackend.Features.Dashboard.DTOs.FirmDashboardDTO - used by every
    /// non-SuperAdmin role. "Scope" is "FirmWide" (FirmAdmin/Partner) or
    /// "AssignedOnly" (AssociateLawyer/Moharrir/InternParalegal).
    /// </summary>
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
}
