using LTSFrontend.Core.Exceptions;
using LTSFrontend.Core.Http;
using LTSFrontend.Features.Firms.DTOs;
using Microsoft.JSInterop;

namespace LTSFrontend.Features.Firms.Services
{
    public class FirmService : IFirmService
    {
        private readonly ApiClient _api;
        private readonly IJSRuntime _js;

        public FirmService(ApiClient api, IJSRuntime js)
        {
            _api = api;
            _js = js;
        }

        public async Task<List<FirmDTO>> GetAllAsync()
        {
            var result = await _api.GetAsync<List<FirmDTO>>(ApiEndpoints.Firms.Base_);
            return result ?? new List<FirmDTO>();
        }

        public Task<FirmDTO?> GetByIdAsync(int id) =>
            _api.GetAsync<FirmDTO>(ApiEndpoints.Firms.ById(id));

        public Task<int> CreateAsync(CreateFirmDTO dto) =>
            _api.PostAsync<int>(ApiEndpoints.Firms.Base_, new
            {
                dto.FirmName,
                dto.FirmCode,
                Address = Norm(dto.Address),
                ContactEmail = Norm(dto.ContactEmail),
                ContactPhone = Norm(dto.ContactPhone),
                dto.AdminFullName,
                dto.AdminEmail,
                dto.AdminPassword
            });

        public Task<bool> UpdateAsync(UpdateFirmDTO dto) =>
            _api.PutAsync<bool>(ApiEndpoints.Firms.ById(dto.FirmID), new
            {
                dto.FirmID,
                dto.FirmName,
                Address = Norm(dto.Address),
                ContactEmail = Norm(dto.ContactEmail),
                ContactPhone = Norm(dto.ContactPhone),
                CustomDomain = Norm(dto.CustomDomain)
            });

        public Task<bool> BlockAsync(int id, string? reason) =>
            _api.PutAsync<bool>(ApiEndpoints.Firms.Block(id), new { Reason = Norm(reason) });

        public Task<bool> UnblockAsync(int id) =>
            _api.PutAsync<bool>(ApiEndpoints.Firms.Unblock(id));

        public Task<bool> DeleteAsync(int id) =>
            _api.DeleteAsync<bool>(ApiEndpoints.Firms.ById(id));

        public async Task ExportAsync(int id)
        {
            using var response = await _api.GetRawAsync(ApiEndpoints.Firms.Export(id));

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException($"Export failed with status {(int)response.StatusCode}.", (int)response.StatusCode);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/zip";
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? $"firm-{id}-export.zip";

            var base64 = Convert.ToBase64String(bytes);
            await _js.InvokeVoidAsync("ltsDownloadFile", fileName, contentType, base64);
        }

        private static string? Norm(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
