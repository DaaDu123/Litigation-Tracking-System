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
    /// <summary>
    /// Normalized Pakistani mobile number, e.g. "+923001234567". Normalized
    /// via PakistaniFormat.NormalizePhone before every write (registration,
    /// profile completion, profile update) so "03001234567" and
    /// "+923001234567" always collide as the same value for uniqueness.
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(100)]
    public string? Department { get; set; }
    /// <summary>
    /// Pakistani CNIC, normalized and stored as 13 raw digits (no dashes)
    /// via PakistaniFormat.NormalizeCnic so "35202-1234567-1" and
    /// "3520212345671" always collide as the same value for uniqueness.
    /// Format for display with PakistaniFormat.FormatCnic.
    /// </summary>
    [MaxLength(13)]
    public string? CNIC { get; set; }
    /// <summary>
    /// False immediately after Firm Admin approval or Firm User
    /// registration, until the mandatory profile-completion step is
    /// finished. AccountStatusGuardBehavior/ProfileCompletionBehavior use
    /// this single flag everywhere a "is the profile done?" check is
    /// needed - it is never duplicated as separate logic elsewhere.
    /// </summary>
    public bool IsProfileCompleted { get; set; } = true;
    /// <summary>
    /// Firm-membership state: Active | Blocked | Removed (see
    /// MembershipStatuses). Distinct from IsActive/IsDeleted, which are
    /// account-level, not firm-relationship-level.
    /// </summary>
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
    /// <summary>
    /// Retains which Firm this user was last a member of after a Remove,
    /// so a Removed user's history is preserved and a Blocked-then-Removed
    /// user can still be checked against their block history for that
    /// specific firm even after FirmID is cleared.
    /// </summary>
    public int? LastFirmID { get; set; }
    /// <summary>
    /// Firm Admin availability toggle. Meaningful for FirmAdmin users only;
    /// left true/null for everyone else. Always read through
    /// AvailabilityEvaluator, never directly, so an expired InactiveUntilUtc
    /// self-heals on read even before the background service sweeps it.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
    [MaxLength(500)]
    public string? InactiveReason { get; set; }
    public DateTime? InactiveFromUtc { get; set; }
    public DateTime? InactiveUntilUtc { get; set; }
    [MaxLength(100)]
    public string? Designation { get; set; }
    public int? RoleID { get; set; }
    /// <summary>
    /// Firm this user belongs to. Null only for the platform-level
    /// SuperAdmin - every other role must belong to exactly one firm.
    /// </summary>
    public int? FirmID { get; set; }
    public bool IsExternal { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public bool IsReleasedForReuse { get; set; } = false;
    public DateTime? LastLogin { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;          // Added: tracks consecutive failed logins for lockout
   public DateTime? LockoutEndUtc { get; set; }
    public DateTime? PasswordChangedDate { get; set; }          // Added (was in SQL, missing in model)
    [Required, MaxLength(64)]
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    // Foreign Keys & Navigation Properties
    [ForeignKey(nameof(RoleID))]
    public Role? Role { get; set; }
    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }
    // Collections
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<UserOtp> UserOtps { get; set; } = [];
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    public ICollection<LoginHistory> LoginHistories { get; set; } = [];
    /// <summary>
    /// Gets the user's role as an enum, or null if not defined.
    /// </summary>
    public UserRole? GetRole() =>
        RoleID.HasValue && Enum.IsDefined(typeof(UserRole), RoleID.Value)
            ? (UserRole)RoleID.Value
            : null;
}