namespace LTSBackend.Features.CaseCategories.DTOs;

public class CaseCategoryDTO
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    /// <summary>True when FirmID is null, i.e. this is a system-wide category shared
    /// across all firms. Only SuperAdmin can edit/deactivate these - a FirmAdmin can
    /// see and use it on cases, but the update/delete endpoints will reject changes.</summary>
    public bool IsGlobal { get; set; }
}
