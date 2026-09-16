using LTSFrontend.Core.Http;
using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly ApiClient _api;

        public MasterDataService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CaseCategoryDTO>> GetCaseCategoriesAsync(bool activeOnly = true)
        {
            var url = $"{ApiEndpoints.Masters.CaseCategories.Base_}?activeOnly={activeOnly.ToString().ToLowerInvariant()}";
            var result = await _api.GetAsync<List<CaseCategoryDTO>>(url);
            return result ?? new List<CaseCategoryDTO>();
        }

        public async Task<List<CaseStageDTO>> GetCaseStagesAsync(bool activeOnly = true)
        {
            var url = $"{ApiEndpoints.Masters.CaseStages.Base_}?activeOnly={activeOnly.ToString().ToLowerInvariant()}";
            var result = await _api.GetAsync<List<CaseStageDTO>>(url);
            return result ?? new List<CaseStageDTO>();
        }

        public async Task<List<CaseStatusDTO>> GetCaseStatusesAsync(bool activeOnly = true)
        {
            var url = $"{ApiEndpoints.Masters.CaseStatuses.Base_}?activeOnly={activeOnly.ToString().ToLowerInvariant()}";
            var result = await _api.GetAsync<List<CaseStatusDTO>>(url);
            return result ?? new List<CaseStatusDTO>();
        }

        public async Task<List<DepartmentDTO>> GetDepartmentsAsync(bool activeOnly = true)
        {
            var url = $"{ApiEndpoints.Masters.Departments.Base_}?activeOnly={activeOnly.ToString().ToLowerInvariant()}";
            var result = await _api.GetAsync<List<DepartmentDTO>>(url);
            return result ?? new List<DepartmentDTO>();
        }
    }
}
