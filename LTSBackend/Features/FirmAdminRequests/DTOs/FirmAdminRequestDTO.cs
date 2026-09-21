namespace LTSBackend.Features.FirmAdminRequests.DTOs;

public class FirmAdminRequestDTO
{
    public int RequestID { get; set; }
    public string? FirmName { get; set; }
    public string? FirmCode { get; set; }
    public string? Address { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? AdminFullName { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string? AdminPhone { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int? CreatedFirmID { get; set; }
}
