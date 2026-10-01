using LTSFrontend.Features.SuperAdminRequests.DTOs;

namespace LTSFrontend.Features.SuperAdminRequests.Services
{
    /// <summary>
    /// Client-side gateway for the caller's own Super Admin request status.
    /// Query-only (no commands) until a Super Admin request flow exists.
    /// </summary>
    public interface ISuperAdminRequestService
    {
        /// <summary>
        /// The caller's most recent Super Admin request, or null when they
        /// have never made one (or when the backend doesn't expose the
        /// endpoint yet).
        /// </summary>
        Task<SuperAdminRequestDTO?> GetMineAsync();
    }
}
