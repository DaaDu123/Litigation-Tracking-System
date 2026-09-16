using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    /// <summary>
    /// Read-only lookups for master-data dropdowns used across other
    /// modules (Cases, Hearings, etc.). CRUD for each master type lives in
    /// its own dedicated page under Features/Masters/Pages.
    /// </summary>
    public interface IMasterDataService
    {
        Task<List<CaseCategoryDTO>> GetCaseCategoriesAsync(bool activeOnly = true);
        Task<List<CaseStageDTO>> GetCaseStagesAsync(bool activeOnly = true);
        Task<List<CaseStatusDTO>> GetCaseStatusesAsync(bool activeOnly = true);
        Task<List<DepartmentDTO>> GetDepartmentsAsync(bool activeOnly = true);
    }
}
