namespace LTSBackend.Features.Cases.DTOs;


public class CaseWorkflowDTO
{
    public long CaseID { get; set; }


    public bool IsLegacy { get; set; }


    public int? WorkflowTemplateID { get; set; }
    public int? WorkflowTemplateVersion { get; set; }


    public int? CurrentTemplateVersion { get; set; }


    public bool TemplateChangedSinceCreation { get; set; }


    public bool CanGenerate { get; set; }

    public List<CaseWorkflowStageDTO> Stages { get; set; } = [];
    public List<CaseDocumentRequirementDTO> Documents { get; set; } = [];
}

public class CaseWorkflowStageDTO
{
    public int StageID { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int SequenceNo { get; set; }


    public string Status { get; set; } = string.Empty;
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
}

public class CaseDocumentRequirementDTO
{
    public int DocumentTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }


    public bool IsFulfilled { get; set; }


    public int UploadedCount { get; set; }
}
