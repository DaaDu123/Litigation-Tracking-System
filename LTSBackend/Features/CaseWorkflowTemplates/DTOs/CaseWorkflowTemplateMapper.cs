using LTSBackend.Models.Masters;

namespace LTSBackend.Features.CaseWorkflowTemplates.DTOs;

/// <summary>Maps a fully loaded template (Category, DefaultDepartment, InitialStatus, Stages.Stage, Documents.DocumentType) to its DTO.</summary>
public static class CaseWorkflowTemplateMapper
{
    public static CaseWorkflowTemplateDTO ToDto(CaseWorkflowTemplate t) => new()
    {
        TemplateID = t.TemplateID,
        CategoryID = t.CategoryID,
        CategoryName = t.Category.CategoryName,
        DefaultDepartmentID = t.DefaultDepartmentID,
        DefaultDepartmentName = t.DefaultDepartment?.DepartmentName,
        InitialStatusID = t.InitialStatusID,
        InitialStatusName = t.InitialStatus.StatusName,
        IsActive = t.IsActive,
        IsGlobal = t.FirmID == null,
        Stages = t.Stages.OrderBy(s => s.SequenceNo)
            .Select(s => new WorkflowStageItemDTO { StageID = s.StageID, StageName = s.Stage.StageName, SequenceNo = s.SequenceNo }).ToList(),
        RequiredDocuments = t.Documents.Where(d => d.IsRequired).OrderBy(d => d.DocumentType.TypeName)
            .Select(d => new WorkflowDocumentItemDTO { DocumentTypeID = d.DocumentTypeID, TypeName = d.DocumentType.TypeName }).ToList(),
        OptionalDocuments = t.Documents.Where(d => !d.IsRequired).OrderBy(d => d.DocumentType.TypeName)
            .Select(d => new WorkflowDocumentItemDTO { DocumentTypeID = d.DocumentTypeID, TypeName = d.DocumentType.TypeName }).ToList()
    };
}
