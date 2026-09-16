using LTSFrontend.Core.Http;
using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public class CaseStatusService : ICaseStatusService
    {
        private readonly ApiClient _api;

        public CaseStatusService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CaseStatusDTO>> GetAllAsync(string? searchText = null, bool activeOnly = false)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchText))
                query.Add($"searchText={Uri.EscapeDataString(searchText)}");
            query.Add($"activeOnly={activeOnly.ToString().ToLowerInvariant()}");

            var url = ApiEndpoints.Masters.CaseStatuses.Base_ + "?" + string.Join("&", query);
            var result = await _api.GetAsync<List<CaseStatusDTO>>(url);
            return result ?? new List<CaseStatusDTO>();
        }

        public Task<int> CreateAsync(CaseStatusFormDTO form) =>
            _api.PostAsync<int>(ApiEndpoints.Masters.CaseStatuses.Base_, new
            {
                StatusName = form.StatusName.Trim(),
                form.SequenceNo,
                ColorCode = form.ColorCode.Trim(),
                form.IsClosed,
                form.IsActive
            });

        public async Task<bool> UpdateAsync(CaseStatusFormDTO form)
        {
            if (form.StatusID is null)
                throw new InvalidOperationException("StatusID is required to update a case status.");

            return await _api.PutAsync<bool>(ApiEndpoints.Masters.CaseStatuses.ById(form.StatusID.Value), new
            {
                StatusID = form.StatusID.Value,
                StatusName = form.StatusName.Trim(),
                form.SequenceNo,
                ColorCode = form.ColorCode.Trim(),
                form.IsClosed,
                form.IsActive
            });
        }

        public Task<bool> DeleteAsync(int id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Masters.CaseStatuses.ById(id));
    }
}
