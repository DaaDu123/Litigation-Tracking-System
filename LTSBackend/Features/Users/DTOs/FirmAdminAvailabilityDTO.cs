namespace LTSBackend.Features.Users.DTOs;

/// <summary>
/// Deliberately omits anything beyond what a Firm User is meant to see
/// (no email, no CNIC) - just enough to render "Status: Inactive /
/// Reason: ... / Available Again: ...".
/// </summary>
public class FirmAdminAvailabilityDTO
{
    public int FirmAdminUserID { get; set; }
    public string FirmAdminName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public string? Reason { get; set; }
    public DateTime? InactiveFromUtc { get; set; }
    public DateTime? InactiveUntilUtc { get; set; }
}
