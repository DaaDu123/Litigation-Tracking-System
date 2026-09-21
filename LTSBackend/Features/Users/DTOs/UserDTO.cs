namespace LTSBackend.Features.Users.DTOs;

public class UserDTO
{
    public int UserID { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? ProfileImage { get; set; }

    public string? Phone { get; set; }

    public string? Department { get; set; }

    public int? RoleID { get; set; }

    public string? RoleName { get; set; }

    public bool IsActive { get; set; }

    public bool IsLockedOut { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string MembershipStatus { get; set; } = "Active";

    /// <summary>Only meaningful for a FirmAdmin row - see AvailabilityEvaluator.</summary>
    public bool IsAvailable { get; set; } = true;

    public DateTime? InactiveUntilUtc { get; set; }
}