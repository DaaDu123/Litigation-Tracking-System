using LTSFrontend.Core.Http;
using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public class CaseStageService : ICaseStageService
    {
        private readonly ApiClient _api;

        public CaseStageService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CaseStageDTO>> GetAllAsync(string? searchText = null, bool activeOnly = false)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchText))
                query.Add($"searchText={Uri.EscapeDataString(searchText)}");
            query.Add($"activeOnly={activeOnly.ToString().ToLowerInvariant()}");

            var url = ApiEndpoints.Masters.CaseStages.Base_ + "?" + string.Join("&", query);
            var result = await _api.GetAsync<List<CaseStageDTO>>(url);
            return result ?? new List<CaseStageDTO>();
        }

        public Task<int> CreateAsync(CaseStageFormDTO form) =>
            _api.PostAsync<int>(ApiEndpoints.Masters.CaseStages.Base_, new
            {
                StageName = form.StageName.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                form.IsActive
            });

        public async Task<bool> UpdateAsync(CaseStageFormDTO form)
        {
            if (form.StageID is null)
                throw new InvalidOperationException("StageID is required to update a case stage.");

            return await _api.PutAsync<bool>(ApiEndpoints.Masters.CaseStages.ById(form.StageID.Value), new
            {
                StageID = form.StageID.Value,
                StageName = form.StageName.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                form.IsActive
            });
        }

        public Task<bool> DeleteAsync(int id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Masters.CaseStages.ById(id));
    }
}
