using MediatR;

namespace LTSBackend.Features.Cases.Commands.GenerateCaseWorkflow;


public record GenerateCaseWorkflowCommand(long CaseID) : IRequest<bool>;
