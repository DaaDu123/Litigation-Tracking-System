using MediatR;

namespace LTSBackend.Features.CaseWorkflowTemplates.Commands.DeleteCaseWorkflowTemplate;

public record DeleteCaseWorkflowTemplateCommand(int TemplateID) : IRequest<bool>;
