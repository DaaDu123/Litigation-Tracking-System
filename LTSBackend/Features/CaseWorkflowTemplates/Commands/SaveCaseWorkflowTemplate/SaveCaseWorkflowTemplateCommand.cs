using MediatR;

namespace LTSBackend.Features.CaseWorkflowTemplates.Commands.SaveCaseWorkflowTemplate;


public record SaveCaseWorkflowTemplateCommand(
    int CategoryID,
    int? DefaultDepartmentID,
    int InitialStatusID,
    List<int> StageIDs,
    List<int> RequiredDocumentTypeIDs,
    List<int> OptionalDocumentTypeIDs,
    bool IsActive
) : IRequest<int>;
