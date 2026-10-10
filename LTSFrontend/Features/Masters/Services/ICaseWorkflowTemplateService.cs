using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public interface ICaseWorkflowTemplateService
    {
        /// <summary>All templates visible to the firm (FirmAdmin/Partner - Master Data screen).</summary>
        Task<List<CaseWorkflowTemplateDTO>> GetAllAsync(bool activeOnly = false);

        /// <summary>The active template that applies to a category, or null when none is configured (New Case wizard preview).</summary>
        Task<CaseWorkflowTemplateDTO?> GetByCategoryAsync(int categoryId);

        Task<int> SaveAsync(CaseWorkflowTemplateFormDTO form);
        Task<bool> DeleteAsync(int templateId);
    }
}
