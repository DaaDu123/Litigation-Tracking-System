using LTSFrontend.Features.Dashboard.DTOs;

namespace LTSFrontend.Features.Dashboard.Services
{
    /// <summary>
    /// Every role gets its own dashboard shape - call the method matching
    /// the signed-in user's own role (see AuthState/session role claim).
    /// Calling the wrong one isn't a security boundary bypass (the backend
    /// still routes purely off the JWT), it just returns the correct
    /// shape for whoever is actually signed in.
    /// </summary>
    public interface IDashboardService
    {
        Task<SuperAdminDashboardDTO?> GetSuperAdminStatsAsync();
        Task<FirmDashboardDTO?> GetFirmStatsAsync();
    }
}
