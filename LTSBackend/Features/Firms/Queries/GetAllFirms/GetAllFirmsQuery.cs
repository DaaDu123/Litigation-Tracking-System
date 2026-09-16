using LTSBackend.Features.Firms.DTOs;
using MediatR;

namespace LTSBackend.Features.Firms.Queries.GetAllFirms;

public record GetAllFirmsQuery : IRequest<List<FirmDTO>>;
