using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

[Table("FirmAdminRequests")]
public class FirmAdminRequest
{
    [Key]
    public int RequestID { get; set; }

    // ---- Proposed firm details ----
    // No longer collected at submission (the workflow now asks only for
    // Email+Password up front - see SubmitFirmAdminRequestCommand). These
    // are filled with system-generated placeholders on Approve and then
    // set for real by the Firm Admin during mandatory profile completion
    // (CompleteFirmAdminProfileCommand). Columns are kept (rather than
    // removed) so the existing Firm entity/approval flow doesn't need a
    // second, parallel code path.
    [MaxLength(150)]
    public string? FirmName { get; set; }

    [MaxLength(30)]
    public string? FirmCode { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    // ---- Requester (future Firm Admin) details ----
    // AdminFullName is likewise no longer collected up front - filled
    // with a placeholder derived from the email and then set for real
    // during profile completion.
    [MaxLength(150)]
    public string? AdminFullName { get; set; }

    [Required, MaxLength(150)]
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>Hashed at submission time, so Approve just copies it onto the new User - the plaintext password is never stored.</summary>
    [Required, MaxLength(255)]
    public string AdminPasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? AdminPhone { get; set; }

    // ---- Workflow state ----
    /// <summary>Pending | Approved | Rejected</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UserID of the SuperAdmin who approved/rejected this request.</summary>
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    /// <summary>Set to the newly created Firm's ID once approved, for traceability.</summary>
    public int? CreatedFirmID { get; set; }
}
