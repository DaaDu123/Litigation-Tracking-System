namespace LTSBackend.Features.Dashboard.DTOs;

/// <summary>
/// Dashboard for every non-SuperAdmin role (FirmAdmin, Partner,
/// AssociateLawyer, Moharrir, InternParalegal). Never crosses a firm
/// boundary (all underlying queries are firm-scoped via
/// AppDbContext's global query filters).
///
/// "Scope" tells the caller which population the counts below describe:
///  - "FirmWide"     : FirmAdmin/Partner (SRS "View Firm Case Directory")
///                     - every case in their own firm.
///  - "AssignedOnly" : AssociateLawyer/Moharrir/InternParalegal - only
///                     cases they are actively assigned to. This is the
///                     same rule GetAllCasesHandler enforces for case
///                     listing, applied here too so a lawyer's dashboard
///                     can never imply visibility into cases they aren't
///                     assigned to.
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

    /// <summary>Only populated (non-null) for FirmWide scope - firm's own user headcount is FirmAdmin/Partner business, not a lawyer's.</summary>
    public int? TotalFirmUsers { get; set; }
}
