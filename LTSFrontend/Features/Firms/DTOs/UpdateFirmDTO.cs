namespace LTSFrontend.Features.Firms.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Firms.Commands.UpdateFirm.UpdateFirmCommand</summary>
    public class UpdateFirmDTO
    {
        public int FirmID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? CustomDomain { get; set; }
    }
}
