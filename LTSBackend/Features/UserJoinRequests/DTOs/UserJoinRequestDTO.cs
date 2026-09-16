namespace LTSBackend.Features.UserJoinRequests.DTOs;

public class UserJoinRequestDTO
{
    public int RequestID { get; set; }
    public int FirmID { get; set; }
    public string? FirmName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public int RequestedRoleID { get; set; }
    public string? RequestedRoleName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int? CreatedUserID { get; set; }
}
