namespace LTSBackend.Features.Cases.DTOs;


public class CreateCaseResultDTO
{
    public long CaseID { get; set; }
    public string InternalReferenceNo { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }


    public bool UsedWorkflowTemplate { get; set; }

    public List<CaseWorkflowStageDTO> Stages { get; set; } = [];
    public List<CaseDocumentRequirementDTO> Documents { get; set; } = [];
}
