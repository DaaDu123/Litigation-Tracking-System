using LTSFrontend.Core.Http;
using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public class CaseCategoryService : ICaseCategoryService
    {
        private readonly ApiClient _api;

        public CaseCategoryService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CaseCategoryDTO>> GetAllAsync(string? searchText = null, bool activeOnly = false)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchText))
                query.Add($"searchText={Uri.EscapeDataString(searchText)}");
            query.Add($"activeOnly={activeOnly.ToString().ToLowerInvariant()}");

            var url = ApiEndpoints.Masters.CaseCategories.Base_ + "?" + string.Join("&", query);
            var result = await _api.GetAsync<List<CaseCategoryDTO>>(url);
            return result ?? new List<CaseCategoryDTO>();
        }

        public Task<int> CreateAsync(CaseCategoryFormDTO form) =>
            _api.PostAsync<int>(ApiEndpoints.Masters.CaseCategories.Base_, new
            {
                CategoryName = form.CategoryName.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                form.IsActive
            });

        public async Task<bool> UpdateAsync(CaseCategoryFormDTO form)
        {
            if (form.CategoryID is null)
                throw new InvalidOperationException("CategoryID is required to update a case category.");

            return await _api.PutAsync<bool>(ApiEndpoints.Masters.CaseCategories.ById(form.CategoryID.Value), new
            {
                CategoryID = form.CategoryID.Value,
                CategoryName = form.CategoryName.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                form.IsActive
            });
        }

        public Task<bool> DeleteAsync(int id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Masters.CaseCategories.ById(id));
    }
}
