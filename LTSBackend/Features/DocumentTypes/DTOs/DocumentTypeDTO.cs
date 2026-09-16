namespace LTSBackend.Features.DocumentTypes.DTOs;

public class DocumentTypeDTO
{
    public int DocumentTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    /// <summary>True = system-wide record shared across all firms (FirmID is null). Only SuperAdmin can edit/deactivate/delete these.</summary>
    public bool IsGlobal { get; set; }
}
