using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDTO>> GetAllAsync(bool activeOnly = false);
        Task<int> CreateAsync(DepartmentFormDTO form);
        Task<bool> UpdateAsync(DepartmentFormDTO form);
        Task<bool> DeleteAsync(int id);
    }
}
