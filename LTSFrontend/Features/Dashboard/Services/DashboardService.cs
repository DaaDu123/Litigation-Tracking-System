using LTSFrontend.Core.Http;
using LTSFrontend.Features.Dashboard.DTOs;

namespace LTSFrontend.Features.Dashboard.Services
{
    public class DashboardService(ApiClient api) : IDashboardService
    {





        public Task<SuperAdminDashboardDTO?> GetSuperAdminStatsAsync()
        {
            return api.GetAsync<SuperAdminDashboardDTO>(ApiEndpoints.Dashboard.Stats);
        }

        public Task<FirmDashboardDTO?> GetFirmStatsAsync()
        {
            return api.GetAsync<FirmDashboardDTO>(ApiEndpoints.Dashboard.Stats);
        }
    }
}