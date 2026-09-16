using LTSBackend.Features.Dashboard.DTOs;
using MediatR;

namespace LTSBackend.Features.Dashboard.Queries.GetSuperAdminDashboard;

public record GetSuperAdminDashboardQuery : IRequest<SuperAdminDashboardDTO>;
