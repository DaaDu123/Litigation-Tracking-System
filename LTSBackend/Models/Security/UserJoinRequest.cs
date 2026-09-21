using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

/// <summary>
/// A public, self-service request from someone who wants to join an
/// EXISTING firm as Partner / Associate Lawyer / Moharrir / Intern
/// Paralegal. Mirrors <see cref="FirmAdminRequest"/> one tier down: that
/// request goes to a SuperAdmin and (on approval) creates a Firm + its
/// first FirmAdmin; this one goes to the target firm's FirmAdmin(s) and
/// (on approval) creates a firm-scoped User with the requested role.
///
/// This is the ONLY way non-FirmAdmin roles get onto a firm through
/// self-service - the FirmAdmin's own "Create User" screen (Users
/// feature) remains a separate, direct-create path that still adds a
/// user immediately with no approval step.
/// </summary>
[Table("UserJoinRequests")]
public class UserJoinRequest
{
    [Key]
    public int RequestID { get; set; }

    /// <summary>The firm the requester wants to join - chosen by them from the public firm/Firm-Admin directory.</summary>
    public int FirmID { get; set; }

    /// <summary>
    /// The already-registered Firm User submitting this request (new flow:
    /// registration and firm-request are two separate steps - see
    /// RegisterCommand + SubmitUserJoinRequestCommand). Null only for
    /// legacy rows created before this field existed, where the account
    /// itself wasn't created until Approve.
    /// </summary>
    public int? UserID { get; set; }

    // ---- Requester details (legacy / display convenience) ----
    // No longer required at submission time in the new flow - populated
    // from the already-registered User's own record for convenient
    // listing without an extra join. Kept nullable so this entity still
    // supports any pre-existing Pending rows from before this change.
    [MaxLength(150)]
    public string? FullName { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    /// <summary>Legacy only. New submissions no longer collect a password here - the requester already has an account and password.</summary>
    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    /// <summary>
    /// Legacy only. New submissions leave this at 0/unset - the accepted
    /// role is always InternParalegal by default (business rule), with
    /// the Firm Admin free to change it afterward via ChangeFirmUserRole.
    /// </summary>
    public int RequestedRoleID { get; set; }

    // ---- Workflow state ----
    /// <summary>Pending | Approved | Rejected | Cancelled</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UserID of the FirmAdmin who approved/rejected this request, or the requester themself for Cancelled.</summary>
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    /// <summary>Set to the User's ID once approved, for traceability (same as UserID once approved, kept for legacy-row compatibility).</summary>
    public int? CreatedUserID { get; set; }

    // Navigation
    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }
}
