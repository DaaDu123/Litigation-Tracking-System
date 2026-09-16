using LTSBackend.Features.CaseStatuses.DTOs;
using MediatR;

namespace LTSBackend.Features.CaseStatuses.Queries.GetAllCaseStatuses;

public sealed record GetAllCaseStatusesQuery(string? SearchText = null, bool ActiveOnly = true) : IRequest<List<CaseStatusDTO>>;
