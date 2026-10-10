using MediatR;

namespace LTSBackend.Features.Cases.Commands.GenerateCaseWorkflow;

/// <summary>Backfill: builds the workflow stages + document checklist for an existing (legacy) case that has none.</summary>
public record GenerateCaseWorkflowCommand(long CaseID) : IRequest<bool>;
