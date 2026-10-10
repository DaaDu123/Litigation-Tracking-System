namespace LTSBackend.Features.CaseWorkflowTemplates.DTOs;

public class CaseWorkflowTemplateDTO
{
    public int TemplateID { get; set; }
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? DefaultDepartmentID { get; set; }
    public string? DefaultDepartmentName { get; set; }
    public int InitialStatusID { get; set; }
    public string InitialStatusName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>True when the template is global (FirmID null) - only SuperAdmin can change it.</summary>
    public bool IsGlobal { get; set; }

    public List<WorkflowStageItemDTO> Stages { get; set; } = [];
    public List<WorkflowDocumentItemDTO> RequiredDocuments { get; set; } = [];
    public List<WorkflowDocumentItemDTO> OptionalDocuments { get; set; } = [];
}

public class WorkflowStageItemDTO
{
    public int StageID { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int SequenceNo { get; set; }
}

public class WorkflowDocumentItemDTO
{
    public int DocumentTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
}
