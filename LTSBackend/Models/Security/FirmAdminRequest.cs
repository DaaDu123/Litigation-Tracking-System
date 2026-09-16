using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

[Table("FirmAdminRequests")]
public class FirmAdminRequest
{
    [Key]
    public int RequestID { get; set; }

    // ---- Proposed firm details ----
    [Required, MaxLength(150)]
    public string FirmName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string FirmCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    // ---- Requester (future Firm Admin) details ----
    [Required, MaxLength(150)]
    public string AdminFullName { get; set; } = string.Empty;

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
