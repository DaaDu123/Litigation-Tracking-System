namespace LTSFrontend.Features.Firms.DTOs
{
    /// <summary>
    /// Form model for provisioning a brand-new firm workspace, which also
    /// bootstraps its first Firm Admin account in the same operation
    /// (mirrors LTSBackend.Features.Firms.Commands.CreateFirm.CreateFirmCommand).
    /// </summary>
    public class CreateFirmDTO
    {
        public string FirmName { get; set; } = string.Empty;
        public string FirmCode { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
    }
}
