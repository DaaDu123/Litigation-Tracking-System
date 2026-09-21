namespace LTSBackend.Features.Users.DTOs;

public class BlockedFirmUserDTO
{
    public int UserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? BlockedAtUtc { get; set; }
    public string? BlockedReason { get; set; }
    public string? BlockedByName { get; set; }
}
