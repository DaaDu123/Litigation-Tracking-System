namespace LTSFrontend.Features.UserJoinRequests.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.UserJoinRequests.DTOs.JoinableFirmDTO</summary>
    public class JoinableFirmDTO
    {
        public int FirmID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string? FirmAdminName { get; set; }
        public string? FirmAdminContactNumber { get; set; }
        public bool IsFirmAdminAvailable { get; set; } = true;
        public DateTime? FirmAdminAvailableAgainAtUtc { get; set; }
    }
}
