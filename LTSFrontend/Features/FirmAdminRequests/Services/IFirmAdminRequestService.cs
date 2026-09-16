using LTSFrontend.Features.FirmAdminRequests.DTOs;

namespace LTSFrontend.Features.FirmAdminRequests.Services
{
    /// <summary>
    /// Client-side gateway to LTSBackend's FirmAdminRequestsController.
    /// SubmitAsync is public/unauthenticated (like AuthService.RegisterAsync);
    /// the rest require the SuperAdmin role.
    /// </summary>
    public interface IFirmAdminRequestService
    {
        Task<int> SubmitAsync(SubmitFirmAdminRequest request);
        Task<List<FirmAdminRequestDTO>> GetAllAsync(string? status = null);
        Task<int> ApproveAsync(int id);
        Task<bool> RejectAsync(int id, string? reason);
    }
}
