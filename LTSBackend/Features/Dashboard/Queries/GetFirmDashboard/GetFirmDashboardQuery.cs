using LTSBackend.Features.Dashboard.DTOs;
using MediatR;

namespace LTSBackend.Features.Dashboard.Queries.GetFirmDashboard;

public record GetFirmDashboardQuery : IRequest<FirmDashboardDTO>;
