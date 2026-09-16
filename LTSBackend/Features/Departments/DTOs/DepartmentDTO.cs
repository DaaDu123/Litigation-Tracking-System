namespace LTSBackend.Features.Departments.DTOs;

public class DepartmentDTO
{
    public int DepartmentID { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string? DepartmentCode { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    /// <summary>True = system-wide record shared across all firms (FirmID is null). Only SuperAdmin can edit/deactivate/delete these.</summary>
    public bool IsGlobal { get; set; }
}
