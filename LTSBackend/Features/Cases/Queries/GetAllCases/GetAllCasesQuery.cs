using LTSBackend.Comman.Responses;
using LTSBackend.Features.Cases.DTOs;
using MediatR;

namespace LTSBackend.Features.Cases.Queries.GetAllCases;

public record GetAllCasesQuery(
    string? SearchText,
    int? CourtID,
    int? StatusID,
    string? Priority,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<CaseDTO>>;