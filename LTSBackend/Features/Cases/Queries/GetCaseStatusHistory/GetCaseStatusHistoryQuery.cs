using LTSBackend.Features.Cases.DTOs;
using MediatR;

namespace LTSBackend.Features.Cases.Queries.GetCaseStatusHistory;

public record GetCaseStatusHistoryQuery(long CaseID) : IRequest<List<CaseStatusHistoryDTO>>;
