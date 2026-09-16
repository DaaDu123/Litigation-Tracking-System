using LTSFrontend.Features.Permissions.DTOs;

namespace LTSFrontend.Features.Permissions.Services
{
    public interface IPermissionService
    {
        Task<List<PermissionDTO>> GetAllAsync();
        Task<List<PermissionDTO>> GetRolePermissionsAsync(int roleId);
        Task<bool> AssignAsync(int roleId, IEnumerable<int> permissionIds);
    }
}
