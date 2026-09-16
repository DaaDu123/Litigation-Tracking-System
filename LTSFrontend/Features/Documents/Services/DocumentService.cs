using LTSFrontend.Core.Exceptions;
using LTSFrontend.Core.Http;
using LTSFrontend.Features.Documents.DTOs;
using Microsoft.JSInterop;

namespace LTSFrontend.Features.Documents.Services
{
    public class DocumentService : IDocumentService
    {
        // Mirrors the 15MB limit enforced by DocumentsController.UploadDocument.
        private const long MaxFileSizeBytes = 15 * 1024 * 1024;

        private readonly ApiClient _api;
        private readonly IJSRuntime _js;

        public DocumentService(ApiClient api, IJSRuntime js)
        {
            _api = api;
            _js = js;
        }

        public async Task<List<DocumentDTO>> GetByCaseAsync(long caseId)
        {
            var result = await _api.GetAsync<List<DocumentDTO>>(ApiEndpoints.Documents.ByCase(caseId));
            return result ?? new List<DocumentDTO>();
        }

        public Task<DocumentDTO?> GetByIdAsync(long documentId) =>
            _api.GetAsync<DocumentDTO>(ApiEndpoints.Documents.ById(documentId));

        public async Task<UploadDocumentResponseDTO> UploadAsync(UploadDocumentRequest request)
        {
            if (request.File == null)
                throw new ApiException("Please choose a file to upload.");

            if (request.File.Size > MaxFileSizeBytes)
                throw new ApiException("File size is too large. Please reduce the document size to 15MB or less.");

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(request.CaseID.ToString()), "CaseID");
            form.Add(new StringContent((request.DocumentTypeID ?? 0).ToString()), "DocumentTypeID");
            form.Add(new StringContent(request.DocumentName), "DocumentName");
            if (!string.IsNullOrWhiteSpace(request.Remarks))
                form.Add(new StringContent(request.Remarks), "Remarks");

            var stream = request.File.OpenReadStream(MaxFileSizeBytes);
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(request.File.ContentType) ? "application/octet-stream" : request.File.ContentType);
            form.Add(streamContent, "File", request.File.Name);

            var result = await _api.PostFormAsync<UploadDocumentResponseDTO>(ApiEndpoints.Documents.Upload, form);
            return result ?? throw new ApiException("Upload succeeded but the server response could not be read.");
        }

        public async Task DownloadAsync(long documentId)
        {
            using var response = await _api.GetRawAsync(ApiEndpoints.Documents.Download(documentId));

            if (!response.IsSuccessStatusCode)
            {
                var message = response.StatusCode == System.Net.HttpStatusCode.Forbidden
                    ? "You don't have permission to download this document."
                    : $"Download failed with status {(int)response.StatusCode}.";
                throw new ApiException(message, (int)response.StatusCode);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? $"document-{documentId}";

            var base64 = Convert.ToBase64String(bytes);
            await _js.InvokeVoidAsync("ltsDownloadFile", fileName, contentType, base64);
        }

        public Task<bool> DeleteAsync(long documentId) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Documents.ById(documentId));

        public async Task<bool> ApproveAsync(long documentId)
        {
            var result = await _api.PostAsync<bool>(ApiEndpoints.Documents.Approve(documentId));
            return result;
        }
    }
}
