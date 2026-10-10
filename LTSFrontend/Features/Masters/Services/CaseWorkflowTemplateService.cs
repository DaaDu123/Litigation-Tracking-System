using LTSFrontend.Core.Http;
using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public class CaseWorkflowTemplateService : ICaseWorkflowTemplateService
    {
        private readonly ApiClient _api;

        public CaseWorkflowTemplateService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CaseWorkflowTemplateDTO>> GetAllAsync(bool activeOnly = false)
        {
            var url = $"{ApiEndpoints.Masters.CaseWorkflowTemplates.Base_}?activeOnly={activeOnly.ToString().ToLowerInvariant()}";
            var result = await _api.GetAsync<List<CaseWorkflowTemplateDTO>>(url);
            return result ?? new List<CaseWorkflowTemplateDTO>();
        }

        public Task<CaseWorkflowTemplateDTO?> GetByCategoryAsync(int categoryId) =>
            _api.GetAsync<CaseWorkflowTemplateDTO>(ApiEndpoints.Masters.CaseWorkflowTemplates.ByCategory(categoryId));

        public Task<int> SaveAsync(CaseWorkflowTemplateFormDTO form) =>
            _api.PostAsync<int>(ApiEndpoints.Masters.CaseWorkflowTemplates.Base_, new
            {
                form.CategoryID,
                DefaultDepartmentID = form.DefaultDepartmentID > 0 ? form.DefaultDepartmentID : (int?)null,
                form.InitialStatusID,
                StageIDs = form.StageIDs,
                RequiredDocumentTypeIDs = form.RequiredDocumentTypeIDs.ToList(),
                OptionalDocumentTypeIDs = form.OptionalDocumentTypeIDs.ToList(),
                form.IsActive
            });

        public Task<bool> DeleteAsync(int templateId) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Masters.CaseWorkflowTemplates.ById(templateId));
    }
}
