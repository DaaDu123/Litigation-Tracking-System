using MediatR;

namespace LTSBackend.Features.CaseStages.Commands.UpdateCaseStage;

public sealed record UpdateCaseStageCommand(int StageID, string StageName, string? Description, bool IsActive = true) : IRequest<bool>;
