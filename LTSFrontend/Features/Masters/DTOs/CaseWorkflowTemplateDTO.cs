namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.CaseWorkflowTemplates.DTOs.CaseWorkflowTemplateDTO</summary>
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

        /// <summary>True = global template shared by every firm (SuperAdmin-managed).</summary>
        public bool IsGlobal { get; set; }

        public List<WorkflowStageItemDTO> Stages { get; set; } = new();
        public List<WorkflowDocumentItemDTO> RequiredDocuments { get; set; } = new();
        public List<WorkflowDocumentItemDTO> OptionalDocuments { get; set; } = new();
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

    /// <summary>Client-side form model for the Case Workflow Template editor. Mirrors SaveCaseWorkflowTemplateDTO.</summary>
    public class CaseWorkflowTemplateFormDTO
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int DefaultDepartmentID { get; set; }
        public int InitialStatusID { get; set; }

        /// <summary>Ordered stage IDs (first = initial stage).</summary>
        public List<int> StageIDs { get; set; } = new();

        public HashSet<int> RequiredDocumentTypeIDs { get; set; } = new();
        public HashSet<int> OptionalDocumentTypeIDs { get; set; } = new();
        public bool IsActive { get; set; } = true;

        public static CaseWorkflowTemplateFormDTO ForCategory(CaseCategoryDTO category, CaseWorkflowTemplateDTO? existing) =>
            existing == null
                ? new CaseWorkflowTemplateFormDTO { CategoryID = category.CategoryID, CategoryName = category.CategoryName }
                : new CaseWorkflowTemplateFormDTO
                {
                    CategoryID = category.CategoryID,
                    CategoryName = category.CategoryName,
                    DefaultDepartmentID = existing.DefaultDepartmentID ?? 0,
                    InitialStatusID = existing.InitialStatusID,
                    StageIDs = existing.Stages.OrderBy(s => s.SequenceNo).Select(s => s.StageID).ToList(),
                    RequiredDocumentTypeIDs = existing.RequiredDocuments.Select(d => d.DocumentTypeID).ToHashSet(),
                    OptionalDocumentTypeIDs = existing.OptionalDocuments.Select(d => d.DocumentTypeID).ToHashSet(),
                    IsActive = existing.IsActive
                };
    }
}
