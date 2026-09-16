using LTSFrontend.Core.Http;
using LTSFrontend.Features.Roles.DTOs;

namespace LTSFrontend.Features.Roles.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApiClient _api;

        public RoleService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<RoleDTO>> GetAllAsync()
        {
            var result = await _api.GetAsync<List<RoleDTO>>(ApiEndpoints.Roles.Base_);
            return result ?? new List<RoleDTO>();
        }

        public Task<RoleDTO?> GetByIdAsync(int id) =>
            _api.GetAsync<RoleDTO>(ApiEndpoints.Roles.ById(id));

        public Task<int> CreateAsync(RoleFormDTO form) =>
            _api.PostAsync<int>(ApiEndpoints.Roles.Base_, new
            {
                RoleName = form.RoleName.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                PermissionIds = form.PermissionIds.ToList()
            });

        public Task<bool> UpdateAsync(RoleFormDTO form) =>
            _api.PutAsync<bool>(ApiEndpoints.Roles.ById(form.RoleID!.Value), new
            {
                RoleID = form.RoleID!.Value,
                RoleName = form.RoleName.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                PermissionIds = form.PermissionIds.ToList()
            });

        public Task<bool> DeleteAsync(int id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Roles.ById(id));
    }
}
