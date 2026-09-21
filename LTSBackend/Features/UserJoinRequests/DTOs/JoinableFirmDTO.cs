namespace LTSBackend.Features.UserJoinRequests.DTOs;

/// <summary>
/// Deliberately minimal (no address/internal details) - this is returned
/// from an [Authorize] (any authenticated, not-yet-in-a-firm user)
/// endpoint so the Firm User dashboard can present an "Available Firm
/// Admins" directory to send a request to. Includes just enough of the
/// Firm Admin's own public profile + live availability to match the
/// spec's "Name / Contact / Status: Available" card - never anything
/// sensitive (no email, no CNIC, no financial data).
/// </summary>
public class JoinableFirmDTO
{
    public int FirmID { get; set; }
    public string FirmName { get; set; } = string.Empty;
    public string? FirmAdminName { get; set; }
    public string? FirmAdminContactNumber { get; set; }
    public bool IsFirmAdminAvailable { get; set; } = true;
    public DateTime? FirmAdminAvailableAgainAtUtc { get; set; }
}
