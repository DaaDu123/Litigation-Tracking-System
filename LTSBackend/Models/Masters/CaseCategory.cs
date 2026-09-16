using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Masters;

[Table("CaseCategories")]
public class CaseCategory
{
    [Key]
    public int CategoryID { get; set; }

    /// <summary>
    /// NULL = system-wide global category (SuperAdmin-managed, visible to
    /// every firm - all existing seeded rows are this). Non-null = a
    /// firm's own custom category, visible/editable only by that firm.
    /// Same pattern as Court.FirmID / Department.FirmID.
    /// </summary>
    public int? FirmID { get; set; }

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [Required]
    [MaxLength(150)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
