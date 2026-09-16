using MediatR;

namespace LTSBackend.Features.CaseStages.Commands.CreateCaseStage;

public sealed record CreateCaseStageCommand(string StageName, string? Description, bool IsActive = true) : IRequest<int>;
