using LTSBackend.Comman.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

public class User
{
    [Key]
    public int UserID { get; set; }
    [Required, MaxLength(50)]
    public string EmployeeNo { get; set; } = string.Empty;
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? ProfileImage { get; set; }






    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(100)]
    public string? Department { get; set; }






    [MaxLength(13)]
    public string? CNIC { get; set; }







    public bool IsProfileCompleted { get; set; } = true;





    [MaxLength(20)]
    public string MembershipStatus { get; set; } = MembershipStatuses.Active;
    [MaxLength(500)]
    public string? MembershipBlockedReason { get; set; }
    public DateTime? MembershipBlockedAtUtc { get; set; }
    public int? MembershipBlockedByUserID { get; set; }
    [MaxLength(500)]
    public string? MembershipRemovedReason { get; set; }
    public DateTime? MembershipRemovedAtUtc { get; set; }
    public int? MembershipRemovedByUserID { get; set; }






    public int? LastFirmID { get; set; }






    public bool IsAvailable { get; set; } = true;
    [MaxLength(500)]
    public string? InactiveReason { get; set; }
    public DateTime? InactiveFromUtc { get; set; }
    public DateTime? InactiveUntilUtc { get; set; }
    [MaxLength(100)]
    public string? Designation { get; set; }
    public int? RoleID { get; set; }




    public int? FirmID { get; set; }
    public bool IsExternal { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public bool IsReleasedForReuse { get; set; } = false;
    public DateTime? LastLogin { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
   public DateTime? LockoutEndUtc { get; set; }
    public DateTime? PasswordChangedDate { get; set; }
    [Required, MaxLength(64)]
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(RoleID))]
    public Role? Role { get; set; }
    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<UserOtp> UserOtps { get; set; } = [];
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    public ICollection<LoginHistory> LoginHistories { get; set; } = [];



    public UserRole? GetRole() =>
        RoleID.HasValue && Enum.IsDefined(typeof(UserRole), RoleID.Value)
            ? (UserRole)RoleID.Value
            : null;
}