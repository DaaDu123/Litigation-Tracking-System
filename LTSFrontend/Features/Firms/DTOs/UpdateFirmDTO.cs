namespace LTSFrontend.Features.Firms.DTOs
{

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
