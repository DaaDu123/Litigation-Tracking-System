using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTSBackend.Models.Security;

namespace LTSBackend.Models.Masters;

[Table("Departments")]
public class Department
{
    [Key]
    public int DepartmentID { get; set; }

    /// <summary>
    /// ARCHITECTURE FIX: same rationale as Court.FirmID - NULL = system-wide
    /// global department (SuperAdmin-managed, visible to every firm; all
    /// existing seeded rows become this), non-null = a firm's own custom
    /// department (visible/editable only by that firm's FirmAdmin+).
    /// </summary>
    public int? FirmID { get; set; }

    [ForeignKey(nameof(FirmID))]
    public Firm? Firm { get; set; }

    [Required]
    [MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? DepartmentCode { get; set; }

    public bool IsActive { get; set; } = true;
}