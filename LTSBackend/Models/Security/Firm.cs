using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTSBackend.Models.Security;

/// <summary>
/// A law firm's isolated workspace. Every non-SuperAdmin user and every
/// Case belongs to exactly one Firm - this is the multi-tenancy boundary
/// (Roles SRS §1/§4.I: Super Admin provisions/blocks/removes firm
/// workspaces; all other roles operate strictly inside their own firm).
/// </summary>
[Table("Firms")]
public class Firm
{
    [Key]
    public int FirmID { get; set; }

    [Required, MaxLength(150)]
    public string FirmName { get; set; } = string.Empty;

    /// <summary>
    /// Short unique code the firm's users register/are invited with
    /// (e.g. "ACME-LAW"). Also usable later as a subdomain slug for
    /// the "migrate to firm's own domain" requirement.
    /// </summary>
    [Required, MaxLength(30)]
    public string FirmCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    /// <summary>Custom domain the firm's data may be migrated to (placeholder for future DNS/migration automation).</summary>
    [MaxLength(150)]
    public string? CustomDomain { get; set; }

    /// <summary>
    /// Domain/data migration status: "None", "Requested", "InProgress",
    /// "Completed", "Failed". Set via POST /api/firms/{id}/migrate-domain
    /// (Roles SRS §4.I: Super Admin "Domain Migration" responsibility).
    /// </summary>
    [MaxLength(30)]
    public string MigrationStatus { get; set; } = "None";

    public DateTime? MigrationRequestedAt { get; set; }
    public int? MigrationRequestedBy { get; set; }
    public DateTime? MigrationCompletedAt { get; set; }

    [MaxLength(500)]
    public string? MigrationNotes { get; set; }

    public bool IsBlocked { get; set; } = false;
    [MaxLength(255)]
    public string? BlockedReason { get; set; }
    public DateTime? BlockedAt { get; set; }
    public int? BlockedBy { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = [];
}