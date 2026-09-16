using LTSBackend.Features.CaseStages.DTOs;
using MediatR;

namespace LTSBackend.Features.CaseStages.Queries.GetAllCaseStages;

public sealed record GetAllCaseStagesQuery(string? SearchText = null, bool ActiveOnly = true) : IRequest<List<CaseStageDTO>>;
