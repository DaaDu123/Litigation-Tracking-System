using LTSFrontend.Features.Documents.DTOs;

namespace LTSFrontend.Features.Documents.Services
{
    public interface IDocumentService
    {
        Task<List<DocumentDTO>> GetByCaseAsync(long caseId);
        Task<DocumentDTO?> GetByIdAsync(long documentId);
        Task<UploadDocumentResponseDTO> UploadAsync(UploadDocumentRequest request);
        Task DownloadAsync(long documentId);
        Task<bool> DeleteAsync(long documentId);
        Task<bool> ApproveAsync(long documentId);
    }
}
