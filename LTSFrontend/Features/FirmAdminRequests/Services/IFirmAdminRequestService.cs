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

        // Authenticated "Create Firm" flow (sidebar modal) - submits from
        // the caller's own already-registered account. No email/password
        // required, and does NOT create a second account.
        Task<int> SubmitFromAccountAsync();
        Task<FirmAdminRequestDTO?> GetMineAsync();

        /// <summary>Requester cancels their OWN pending "Create Firm" request (frees their single access-request slot).</summary>
        Task<bool> CancelAsync(int requestId);
    }
}
