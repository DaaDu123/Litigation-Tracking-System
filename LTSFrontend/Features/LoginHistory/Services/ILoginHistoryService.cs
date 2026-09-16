using LTSFrontend.Core.DTOs;
using LTSFrontend.Features.LoginHistory.DTOs;

namespace LTSFrontend.Features.LoginHistory.Services
{
    public interface ILoginHistoryService
    {
        Task<PagedResult<LoginHistoryDTO>> GetAllAsync(LoginHistoryFilterDTO filter);
        Task<List<MyLoginHistoryDTO>> GetMyHistoryAsync();
    }
}
