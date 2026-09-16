namespace LTSBackend.Features.Firms.DTOs;

public class FirmDTO
{
    public int FirmID { get; set; }
    public string FirmName { get; set; } = string.Empty;
    public string FirmCode { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? CustomDomain { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockedReason { get; set; }
    public DateTime? BlockedAt { get; set; }
    public int UserCount { get; set; }
    public int CaseCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
