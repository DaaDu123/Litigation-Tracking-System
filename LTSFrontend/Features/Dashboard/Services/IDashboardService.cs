using LTSFrontend.Features.Dashboard.DTOs;

namespace LTSFrontend.Features.Dashboard.Services
{







    public interface IDashboardService
    {
        Task<SuperAdminDashboardDTO?> GetSuperAdminStatsAsync();
        Task<FirmDashboardDTO?> GetFirmStatsAsync();
    }
}
