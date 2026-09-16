using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public interface IDocumentTypeService
    {
        Task<List<DocumentTypeDTO>> GetAllAsync(string? searchText = null, bool activeOnly = false);
        Task<int> CreateAsync(DocumentTypeFormDTO form);
        Task<bool> UpdateAsync(DocumentTypeFormDTO form);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Lightweight {DocumentTypeID, TypeName} list for the Upload Document
        /// dropdown. Reachable by every role that can upload a document, unlike
        /// GetAllAsync which is FirmAdmin/Partner (master-data admin) only.
        /// </summary>
        Task<List<DocumentTypeOptionDTO>> GetOptionsAsync();
    }
}
