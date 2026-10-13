namespace LTSBackend.Features.UserJoinRequests.DTOs;










public class JoinableFirmDTO
{
    public int FirmID { get; set; }
    public string FirmName { get; set; } = string.Empty;
    public string? FirmAdminName { get; set; }
    public string? FirmAdminContactNumber { get; set; }
    public bool IsFirmAdminAvailable { get; set; } = true;
    public DateTime? FirmAdminAvailableAgainAtUtc { get; set; }
}
