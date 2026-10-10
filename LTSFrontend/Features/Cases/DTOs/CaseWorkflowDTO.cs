namespace LTSFrontend.Features.Cases.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Cases.DTOs.CaseWorkflowDTO - a case's own workflow progress + document checklist.</summary>
    public class CaseWorkflowDTO
    {
        public long CaseID { get; set; }

        /// <summary>True when the case has no workflow rows (created before workflow templates existed).</summary>
        public bool IsLegacy { get; set; }
        public int? WorkflowTemplateID { get; set; }
        public int? WorkflowTemplateVersion { get; set; }
        public int? CurrentTemplateVersion { get; set; }
        public bool TemplateChangedSinceCreation { get; set; }

        /// <summary>A workflow can be generated now (legacy case + an active template exists).</summary>
        public bool CanGenerate { get; set; }
        public List<CaseWorkflowStageDTO> Stages { get; set; } = new();
        public List<CaseDocumentRequirementDTO> Documents { get; set; } = new();
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
        public bool IsFulfilled { get; set; }
        public int UploadedCount { get; set; }
    }

    /// <summary>Mirrors LTSBackend.Features.Cases.DTOs.CreateCaseResultDTO - what Create Case returns.</summary>
    public class CreateCaseResultDTO
    {
        public long CaseID { get; set; }
        public string InternalReferenceNo { get; set; } = string.Empty;
        public string CaseNumber { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public bool UsedWorkflowTemplate { get; set; }
        public List<CaseWorkflowStageDTO> Stages { get; set; } = new();
        public List<CaseDocumentRequirementDTO> Documents { get; set; } = new();
    }
}
