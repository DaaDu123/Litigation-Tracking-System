using MediatR;

namespace LTSBackend.Features.CaseStages.Commands.DeleteCaseStage;

public sealed record DeleteCaseStageCommand(int StageID) : IRequest<bool>;
