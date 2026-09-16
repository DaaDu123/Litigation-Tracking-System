using LTSFrontend.Core.Http;
using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApiClient _api;

        public DepartmentService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<DepartmentDTO>> GetAllAsync(bool activeOnly = false)
        {
            var url = ApiEndpoints.Masters.Departments.Base_ + $"?activeOnly={activeOnly.ToString().ToLowerInvariant()}";
            var result = await _api.GetAsync<List<DepartmentDTO>>(url);
            return result ?? new List<DepartmentDTO>();
        }

        public Task<int> CreateAsync(DepartmentFormDTO form) =>
            _api.PostAsync<int>(ApiEndpoints.Masters.Departments.Base_, new
            {
                DepartmentName = form.DepartmentName.Trim(),
                DepartmentCode = string.IsNullOrWhiteSpace(form.DepartmentCode) ? null : form.DepartmentCode.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                form.IsActive
            });

        public async Task<bool> UpdateAsync(DepartmentFormDTO form)
        {
            if (form.DepartmentID is null)
                throw new InvalidOperationException("DepartmentID is required to update a department.");

            return await _api.PutAsync<bool>(ApiEndpoints.Masters.Departments.ById(form.DepartmentID.Value), new
            {
                DepartmentID = form.DepartmentID.Value,
                DepartmentName = form.DepartmentName.Trim(),
                DepartmentCode = string.IsNullOrWhiteSpace(form.DepartmentCode) ? null : form.DepartmentCode.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                form.IsActive
            });
        }

        public Task<bool> DeleteAsync(int id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Masters.Departments.ById(id));
    }
}
