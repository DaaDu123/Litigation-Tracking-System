using LTSFrontend.Core.Http;
using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public class CourtService : ICourtService
    {
        private readonly ApiClient _api;

        public CourtService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CourtDTO>> GetAllAsync(string? searchText = null, bool activeOnly = false)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchText))
                query.Add($"searchText={Uri.EscapeDataString(searchText)}");
            query.Add($"activeOnly={activeOnly.ToString().ToLowerInvariant()}");

            var url = ApiEndpoints.Masters.Courts.Base_ + "?" + string.Join("&", query);
            var result = await _api.GetAsync<List<CourtDTO>>(url);
            return result ?? new List<CourtDTO>();
        }

        public Task<int> CreateAsync(CourtFormDTO form) =>
            _api.PostAsync<int>(ApiEndpoints.Masters.Courts.Base_, new
            {
                CourtName = form.CourtName.Trim(),
                CourtType = string.IsNullOrWhiteSpace(form.CourtType) ? null : form.CourtType.Trim(),
                Jurisdiction = string.IsNullOrWhiteSpace(form.Jurisdiction) ? null : form.Jurisdiction.Trim(),
                Address = string.IsNullOrWhiteSpace(form.Address) ? null : form.Address.Trim(),
                form.IsActive
            });

        public async Task<bool> UpdateAsync(CourtFormDTO form)
        {
            if (form.CourtID is null)
                throw new InvalidOperationException("CourtID is required to update a court.");

            return await _api.PutAsync<bool>(ApiEndpoints.Masters.Courts.ById(form.CourtID.Value), new
            {
                CourtID = form.CourtID.Value,
                CourtName = form.CourtName.Trim(),
                CourtType = string.IsNullOrWhiteSpace(form.CourtType) ? null : form.CourtType.Trim(),
                Jurisdiction = string.IsNullOrWhiteSpace(form.Jurisdiction) ? null : form.Jurisdiction.Trim(),
                Address = string.IsNullOrWhiteSpace(form.Address) ? null : form.Address.Trim(),
                form.IsActive
            });
        }

        public Task<bool> DeleteAsync(int id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Masters.Courts.ById(id));
    }
}
