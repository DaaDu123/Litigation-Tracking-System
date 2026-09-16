using LTSBackend.Features.Courts.DTOs;
using MediatR;

namespace LTSBackend.Features.Courts.Queries.GetCourtById;

public sealed record GetCourtByIdQuery(int CourtID) : IRequest<CourtDTO>;
