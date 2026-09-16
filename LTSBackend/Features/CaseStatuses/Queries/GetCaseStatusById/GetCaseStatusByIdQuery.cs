using LTSBackend.Features.CaseStatuses.DTOs;
using MediatR;

namespace LTSBackend.Features.CaseStatuses.Queries.GetCaseStatusById;

public sealed record GetCaseStatusByIdQuery(int StatusID) : IRequest<CaseStatusDTO>;
