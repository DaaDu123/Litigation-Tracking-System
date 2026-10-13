using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public interface IDocumentTypeService
    {
        Task<List<DocumentTypeDTO>> GetAllAsync(string? searchText = null, bool activeOnly = false);
        Task<int> CreateAsync(DocumentTypeFormDTO form);
        Task<bool> UpdateAsync(DocumentTypeFormDTO form);
        Task<bool> DeleteAsync(int id);






        Task<List<DocumentTypeOptionDTO>> GetOptionsAsync();
    }
}
