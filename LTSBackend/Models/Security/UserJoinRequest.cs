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

    /// <summary>The firm the requester wants to join - chosen by them from the public firm picker.</summary>
    public int FirmID { get; set; }

    // ---- Requester details ----
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Hashed at submission time, so Approve just copies it onto the new User - the plaintext password is never stored.</summary>
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    /// <summary>
    /// The role being requested. Restricted at submission time (see
    /// RoleHierarchy.IsJoinableRole) to Partner, AssociateLawyer,
    /// Moharrir, InternParalegal - never FirmAdmin or SuperAdmin.
    /// </summary>
    public int RequestedRoleID { get; set; }

    // ---- Workflow state ----
    /// <summary>Pending | Approved | Rejected</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UserID of the FirmAdmin who approved/rejected this request.</summary>
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    /// <summary>Set to the newly created User's ID once approved, for traceability.</summary>
    public int? CreatedUserID { get; set; }

    // Navigation
    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }
}
