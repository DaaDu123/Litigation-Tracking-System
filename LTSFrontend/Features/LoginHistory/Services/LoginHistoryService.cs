using LTSFrontend.Core.Http;
using LTSFrontend.Core.DTOs;
using LTSFrontend.Features.LoginHistory.DTOs;

namespace LTSFrontend.Features.LoginHistory.Services
{
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly ApiClient _api;

        public LoginHistoryService(ApiClient api)
        {
            _api = api;
        }

        public async Task<PagedResult<LoginHistoryDTO>> GetAllAsync(LoginHistoryFilterDTO filter)
        {
            var query = new List<string>
            {
                $"pageNumber={filter.PageNumber}",
                $"pageSize={filter.PageSize}"
            };
            if (!string.IsNullOrWhiteSpace(filter.Search)) query.Add($"search={Uri.EscapeDataString(filter.Search)}");
            if (!string.IsNullOrWhiteSpace(filter.Status)) query.Add($"status={Uri.EscapeDataString(filter.Status)}");
            if (filter.FromDate.HasValue) query.Add($"fromDate={filter.FromDate.Value:yyyy-MM-dd}");
            if (filter.ToDate.HasValue) query.Add($"toDate={filter.ToDate.Value:yyyy-MM-dd}");

            var url = ApiEndpoints.LoginHistory.Base_ + "?" + string.Join("&", query);
            var result = await _api.GetAsync<PagedResult<LoginHistoryDTO>>(url);
            return result ?? new PagedResult<LoginHistoryDTO> { PageNumber = filter.PageNumber, PageSize = filter.PageSize };
        }

        public async Task<List<MyLoginHistoryDTO>> GetMyHistoryAsync()
        {
            var result = await _api.GetAsync<List<MyLoginHistoryDTO>>(ApiEndpoints.LoginHistory.My);
            return result ?? new List<MyLoginHistoryDTO>();
        }
    }
}
