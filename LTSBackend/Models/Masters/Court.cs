using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Masters;

[Table("Courts")]
public class Court
{
    [Key]
    public int CourtID { get; set; }

    /// <summary>
    /// ARCHITECTURE FIX (found during security review): Court previously had
    /// no tenant boundary at all, which let any firm's FirmAdmin rename or
    /// delete a court record that OTHER firms' cases/hearings depended on -
    /// a live cross-tenant data-integrity risk (temporarily mitigated by
    /// restricting mutation to SuperAdmin in CourtsController).
    ///
    /// FirmID is intentionally NULLABLE rather than required:
    ///  - NULL = a system-wide "global" court (e.g. a well-known High
    ///    Court) visible to every firm and managed only by SuperAdmin -
    ///    this is what all existing seeded rows become.
    ///  - Non-null = a firm's own custom court entry, visible only to that
    ///    firm and manageable by that firm's own FirmAdmin+.
    /// See AppDbContext's HasQueryFilter on this entity for the read-side
    /// enforcement, and CreateCourtHandler/UpdateCourtHandler/
    /// DeleteCourtHandler for the write-side ownership checks.
    /// </summary>
    public int? FirmID { get; set; }

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [Required]
    [MaxLength(150)]
    public string CourtName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CourtType { get; set; }

    [MaxLength(200)]
    public string? Jurisdiction { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
