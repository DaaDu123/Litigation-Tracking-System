using LTSFrontend.Features.UserJoinRequests.DTOs;

namespace LTSFrontend.Features.UserJoinRequests.Services
{






    public interface IUserJoinRequestService
    {
        Task<List<JoinableFirmDTO>> GetJoinableFirmsAsync();
        Task<int> SubmitAsync(int firmId);
        Task<UserJoinRequestDTO?> GetMineAsync();
        Task<bool> CancelAsync(int requestId);
        Task<List<UserJoinRequestDTO>> GetAllAsync(string? status = null);
        Task<int> ApproveAsync(int id);
        Task<bool> RejectAsync(int id, string? reason);
    }
}
