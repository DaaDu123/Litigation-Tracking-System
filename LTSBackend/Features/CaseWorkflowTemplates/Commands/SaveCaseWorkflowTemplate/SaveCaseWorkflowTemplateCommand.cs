using MediatR;

namespace LTSBackend.Features.CaseWorkflowTemplates.Commands.SaveCaseWorkflowTemplate;

/// <summary>Creates or replaces the caller's template for a category (one template per category per scope). Returns the TemplateID.</summary>
public record SaveCaseWorkflowTemplateCommand(
    int CategoryID,
    int? DefaultDepartmentID,
    int InitialStatusID,
    List<int> StageIDs,
    List<int> RequiredDocumentTypeIDs,
    List<int> OptionalDocumentTypeIDs,
    bool IsActive
) : IRequest<int>;
