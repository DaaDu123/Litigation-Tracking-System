using LTSFrontend.Core.Http;
using LTSFrontend.Features.Masters.DTOs;

namespace LTSFrontend.Features.Masters.Services
{
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly ApiClient _api;

        public DocumentTypeService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<DocumentTypeDTO>> GetAllAsync(string? searchText = null, bool activeOnly = false)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchText))
                query.Add($"searchText={Uri.EscapeDataString(searchText)}");
            query.Add($"activeOnly={activeOnly.ToString().ToLowerInvariant()}");

            var url = ApiEndpoints.Masters.DocumentTypes.Base_ + "?" + string.Join("&", query);
            var result = await _api.GetAsync<List<DocumentTypeDTO>>(url);
            return result ?? new List<DocumentTypeDTO>();
        }

        public Task<int> CreateAsync(DocumentTypeFormDTO form) =>
            _api.PostAsync<int>(ApiEndpoints.Masters.DocumentTypes.Base_, new
            {
                TypeName = form.TypeName.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                form.IsActive
            });

        public async Task<bool> UpdateAsync(DocumentTypeFormDTO form)
        {
            if (form.DocumentTypeID is null)
                throw new InvalidOperationException("DocumentTypeID is required to update a document type.");

            return await _api.PutAsync<bool>(ApiEndpoints.Masters.DocumentTypes.ById(form.DocumentTypeID.Value), new
            {
                DocumentTypeID = form.DocumentTypeID.Value,
                TypeName = form.TypeName.Trim(),
                Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
                form.IsActive
            });
        }

        public Task<bool> DeleteAsync(int id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Masters.DocumentTypes.ById(id));

        public async Task<List<DocumentTypeOptionDTO>> GetOptionsAsync()
        {
            var result = await _api.GetAsync<List<DocumentTypeOptionDTO>>(ApiEndpoints.Masters.DocumentTypes.Options);
            return result ?? new List<DocumentTypeOptionDTO>();
        }
    }
}
