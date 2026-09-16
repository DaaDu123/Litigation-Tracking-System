namespace LTSBackend.Features.Courts.DTOs;

public class CourtDTO
{
    public int CourtID { get; set; }

    public string CourtName { get; set; } = string.Empty;

    public string? CourtType { get; set; }

    public string? Jurisdiction { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    /// <summary>True = system-wide record shared across all firms (FirmID is null). Only SuperAdmin can edit/deactivate/delete these.</summary>
    public bool IsGlobal { get; set; }
}
