using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public interface ICaseCategoryService
    {
        Task<List<CaseCategoryDTO>> GetAllAsync(string? searchText = null, bool activeOnly = false);
        Task<int> CreateAsync(CaseCategoryFormDTO form);
        Task<bool> UpdateAsync(CaseCategoryFormDTO form);
        Task<bool> DeleteAsync(int id);
    }
}
