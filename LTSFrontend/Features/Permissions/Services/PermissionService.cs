using LTSFrontend.Core.Http;
using LTSFrontend.Features.Permissions.DTOs;

namespace LTSFrontend.Features.Permissions.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApiClient _api;

        public PermissionService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<PermissionDTO>> GetAllAsync()
        {
            var result = await _api.GetAsync<List<PermissionDTO>>(ApiEndpoints.Permissions.Base_);
            return result ?? new List<PermissionDTO>();
        }

        public async Task<List<PermissionDTO>> GetRolePermissionsAsync(int roleId)
        {
            var result = await _api.GetAsync<List<PermissionDTO>>(ApiEndpoints.Permissions.ByRoleId(roleId));
            return result ?? new List<PermissionDTO>();
        }

        public Task<bool> AssignAsync(int roleId, IEnumerable<int> permissionIds) =>
            _api.PutAsync<bool>(ApiEndpoints.Permissions.Assign, new
            {
                RoleID = roleId,
                PermissionIds = permissionIds.ToList()
            });
    }
}
