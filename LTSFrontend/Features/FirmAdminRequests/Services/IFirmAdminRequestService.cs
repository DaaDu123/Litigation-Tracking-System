using LTSFrontend.Features.FirmAdminRequests.DTOs;

namespace LTSFrontend.Features.FirmAdminRequests.Services
{





    public interface IFirmAdminRequestService
    {
        Task<int> SubmitAsync(SubmitFirmAdminRequest request);
        Task<List<FirmAdminRequestDTO>> GetAllAsync(string? status = null);
        Task<int> ApproveAsync(int id);
        Task<bool> RejectAsync(int id, string? reason);




        Task<int> SubmitFromAccountAsync();
        Task<FirmAdminRequestDTO?> GetMineAsync();


        Task<bool> CancelAsync(int requestId);
    }
}
