using LTSFrontend.Features.Roles.DTOs;

namespace LTSFrontend.Features.Roles.Services
{
    public interface IRoleService
    {
        Task<List<RoleDTO>> GetAllAsync();
        Task<RoleDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(RoleFormDTO form);
        Task<bool> UpdateAsync(RoleFormDTO form);
        Task<bool> DeleteAsync(int id);
    }
}
