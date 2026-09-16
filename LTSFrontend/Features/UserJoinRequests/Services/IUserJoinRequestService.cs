using LTSFrontend.Features.UserJoinRequests.DTOs;

namespace LTSFrontend.Features.UserJoinRequests.Services
{
    /// <summary>
    /// Client-side gateway to LTSBackend's UserJoinRequestsController.
    /// GetJoinableFirmsAsync and SubmitAsync are public/unauthenticated
    /// (like AuthService.RegisterAsync); the rest require the FirmAdmin role.
    /// </summary>
    public interface IUserJoinRequestService
    {
        Task<List<JoinableFirmDTO>> GetJoinableFirmsAsync();
        Task<int> SubmitAsync(SubmitUserJoinRequest request);
        Task<List<UserJoinRequestDTO>> GetAllAsync(string? status = null);
        Task<int> ApproveAsync(int id);
        Task<bool> RejectAsync(int id, string? reason);
    }
}
