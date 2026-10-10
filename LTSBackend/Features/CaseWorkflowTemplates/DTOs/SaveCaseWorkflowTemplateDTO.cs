namespace LTSBackend.Features.CaseWorkflowTemplates.DTOs;

public class SaveCaseWorkflowTemplateDTO
{
    public int CategoryID { get; set; }
    public int? DefaultDepartmentID { get; set; }
    public int InitialStatusID { get; set; }

    /// <summary>Stage IDs in workflow order (first = initial stage).</summary>
    public List<int> StageIDs { get; set; } = [];

    public List<int> RequiredDocumentTypeIDs { get; set; } = [];
    public List<int> OptionalDocumentTypeIDs { get; set; } = [];
    public bool IsActive { get; set; } = true;
}
