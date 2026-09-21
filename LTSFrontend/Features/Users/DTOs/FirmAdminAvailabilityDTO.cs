namespace LTSFrontend.Features.Users.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Users.DTOs.FirmAdminAvailabilityDTO</summary>
    public class FirmAdminAvailabilityDTO
    {
        public int FirmAdminUserID { get; set; }
        public string FirmAdminName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public string? Reason { get; set; }
        public DateTime? InactiveFromUtc { get; set; }
        public DateTime? InactiveUntilUtc { get; set; }
    }
}
