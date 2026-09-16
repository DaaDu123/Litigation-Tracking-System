using LTSBackend.Features.Cases.DTOs;
using MediatR;

namespace LTSBackend.Features.Cases.Queries.GetCaseById;

public record GetCaseByIdQuery(long CaseID) : IRequest<CaseDTO?>;