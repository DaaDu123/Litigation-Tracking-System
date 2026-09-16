using LTSFrontend.Core.Http;
using LTSFrontend.Core.DTOs;
using LTSFrontend.Features.AuditLogs.DTOs;

namespace LTSFrontend.Features.AuditLogs.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApiClient _api;

        public AuditLogService(ApiClient api)
        {
            _api = api;
        }

        public async Task<PagedResult<AuditLogDTO>> GetAllAsync(AuditLogFilterDTO filter)
        {
            var query = new List<string>
            {
                $"pageNumber={filter.PageNumber}",
                $"pageSize={filter.PageSize}"
            };
            if (!string.IsNullOrWhiteSpace(filter.Search)) query.Add($"search={Uri.EscapeDataString(filter.Search)}");
            if (!string.IsNullOrWhiteSpace(filter.Action)) query.Add($"action={Uri.EscapeDataString(filter.Action)}");
            if (filter.FromDate.HasValue) query.Add($"fromDate={filter.FromDate.Value:yyyy-MM-dd}");
            if (filter.ToDate.HasValue) query.Add($"toDate={filter.ToDate.Value:yyyy-MM-dd}");

            var url = ApiEndpoints.AuditLogs.Base_ + "?" + string.Join("&", query);
            var result = await _api.GetAsync<PagedResult<AuditLogDTO>>(url);
            return result ?? new PagedResult<AuditLogDTO> { PageNumber = filter.PageNumber, PageSize = filter.PageSize };
        }
    }
}
