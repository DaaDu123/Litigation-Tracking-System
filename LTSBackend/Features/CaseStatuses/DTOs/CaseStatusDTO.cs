namespace LTSBackend.Features.CaseStatuses.DTOs;

public class CaseStatusDTO
{
    public int StatusID { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int SequenceNo { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    public bool IsActive { get; set; }

    /// <summary>True = system-wide record shared across all firms (FirmID is null). Only SuperAdmin can edit/deactivate/delete these.</summary>
    public bool IsGlobal { get; set; }
}
