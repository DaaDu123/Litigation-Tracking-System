using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{





    public interface IMasterDataService
    {
        Task<List<CaseCategoryDTO>> GetCaseCategoriesAsync(bool activeOnly = true);
        Task<List<CaseStageDTO>> GetCaseStagesAsync(bool activeOnly = true);
        Task<List<CaseStatusDTO>> GetCaseStatusesAsync(bool activeOnly = true);
        Task<List<DepartmentDTO>> GetDepartmentsAsync(bool activeOnly = true);
    }
}
