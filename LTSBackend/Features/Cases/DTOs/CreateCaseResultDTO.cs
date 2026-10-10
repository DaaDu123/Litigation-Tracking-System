namespace LTSBackend.Features.Cases.DTOs;

/// <summary>What Create Case returns: the new case plus the workflow + document checklist generated for it.</summary>
public class CreateCaseResultDTO
{
    public long CaseID { get; set; }
    public string InternalReferenceNo { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }

    /// <summary>False when the category had no workflow template (case created with default New status / Filing stage and no checklist).</summary>
    public bool UsedWorkflowTemplate { get; set; }

    public List<CaseWorkflowStageDTO> Stages { get; set; } = [];
    public List<CaseDocumentRequirementDTO> Documents { get; set; } = [];
}
