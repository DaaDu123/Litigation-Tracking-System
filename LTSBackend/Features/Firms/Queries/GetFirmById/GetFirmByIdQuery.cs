using LTSBackend.Features.Firms.DTOs;
using MediatR;

namespace LTSBackend.Features.Firms.Queries.GetFirmById;

public record GetFirmByIdQuery(int FirmID) : IRequest<FirmDTO?>;
