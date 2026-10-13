namespace LTSBackend.Features.Cases.DTOs;







public class CaseStatusHistoryDTO
{
    public long HistoryID { get; set; }
    public long CaseID { get; set; }

    public int? OldStatusID { get; set; }
    public string? OldStatusName { get; set; }

    public int NewStatusID { get; set; }
    public string NewStatusName { get; set; } = string.Empty;

    public int ChangedBy { get; set; }
    public string ChangedByName { get; set; } = string.Empty;

    public DateTime ChangedDate { get; set; }
    public string? Remarks { get; set; }
}
