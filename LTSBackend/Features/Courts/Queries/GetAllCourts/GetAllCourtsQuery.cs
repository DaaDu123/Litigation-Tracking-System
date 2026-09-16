using LTSBackend.Features.Courts.DTOs;
using MediatR;

namespace LTSBackend.Features.Courts.Queries.GetAllCourts;

public sealed record GetAllCourtsQuery(string? SearchText = null,bool ActiveOnly = true ) : IRequest<List<CourtDTO>>;
