namespace LTSBackend.Features.Cases.DTOs;

/// <summary>A case's own workflow progress + document checklist.</summary>
public class CaseWorkflowDTO
{
    public long CaseID { get; set; }

    /// <summary>True when the case has no workflow rows (created before workflow templates existed).</summary>
    public bool IsLegacy { get; set; }

    /// <summary>Template (and revision) the case was generated from; null for legacy / generated-without-template cases.</summary>
    public int? WorkflowTemplateID { get; set; }
    public int? WorkflowTemplateVersion { get; set; }

    /// <summary>Revision of the category's currently active template (null if none) - differs from WorkflowTemplateVersion when the template was edited after this case was created.</summary>
    public int? CurrentTemplateVersion { get; set; }

    /// <summary>True when the template was edited after this case was created. The case keeps its own snapshot; this is informational.</summary>
    public bool TemplateChangedSinceCreation { get; set; }

    /// <summary>True when a workflow can be generated now (legacy case + an active template exists for its category).</summary>
    public bool CanGenerate { get; set; }

    public List<CaseWorkflowStageDTO> Stages { get; set; } = [];
    public List<CaseDocumentRequirementDTO> Documents { get; set; } = [];
}

public class CaseWorkflowStageDTO
{
    public int StageID { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int SequenceNo { get; set; }

    /// <summary>Pending / Active / Completed</summary>
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

    /// <summary>True when at least one approved (non-draft) latest document of this type is uploaded to the case.</summary>
    public bool IsFulfilled { get; set; }

    /// <summary>Number of latest-version documents of this type on the case (drafts included).</summary>
    public int UploadedCount { get; set; }
}
