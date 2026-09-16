using LTSBackend.Features.CaseStages.DTOs;
using MediatR;

namespace LTSBackend.Features.CaseStages.Queries.GetCaseStageById;

public sealed record GetCaseStageByIdQuery(int StageID) : IRequest<CaseStageDTO>;
