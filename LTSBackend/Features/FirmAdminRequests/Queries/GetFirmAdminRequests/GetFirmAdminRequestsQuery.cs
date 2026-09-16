using LTSBackend.Features.FirmAdminRequests.DTOs;
using MediatR;

namespace LTSBackend.Features.FirmAdminRequests.Queries.GetFirmAdminRequests;

public record GetFirmAdminRequestsQuery(string? Status) : IRequest<List<FirmAdminRequestDTO>>;
