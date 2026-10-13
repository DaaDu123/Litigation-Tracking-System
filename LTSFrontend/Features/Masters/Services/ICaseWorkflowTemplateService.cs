using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public interface ICaseWorkflowTemplateService
    {

        Task<List<CaseWorkflowTemplateDTO>> GetAllAsync(bool activeOnly = false);


        Task<CaseWorkflowTemplateDTO?> GetByCategoryAsync(int categoryId);

        Task<int> SaveAsync(CaseWorkflowTemplateFormDTO form);
        Task<bool> DeleteAsync(int templateId);
    }
}
