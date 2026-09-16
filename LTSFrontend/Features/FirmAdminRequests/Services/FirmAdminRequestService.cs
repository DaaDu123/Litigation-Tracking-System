using LTSFrontend.Core.Http;
using LTSFrontend.Features.FirmAdminRequests.DTOs;

namespace LTSFrontend.Features.FirmAdminRequests.Services
{
    public class FirmAdminRequestService : IFirmAdminRequestService
    {
        private readonly ApiClient _api;

        public FirmAdminRequestService(ApiClient api)
        {
            _api = api;
        }

        public Task<int> SubmitAsync(SubmitFirmAdminRequest request) =>
            _api.PostAsync<int>(ApiEndpoints.FirmAdminRequests.Base_, new
            {
                request.FirmName,
                request.FirmCode,
                Address = Norm(request.Address),
                ContactEmail = Norm(request.ContactEmail),
                ContactPhone = Norm(request.ContactPhone),
                request.AdminFullName,
                request.AdminEmail,
                request.AdminPassword,
                AdminPhone = Norm(request.AdminPhone)
            });

        public async Task<List<FirmAdminRequestDTO>> GetAllAsync(string? status = null)
        {
            var result = await _api.GetAsync<List<FirmAdminRequestDTO>>(ApiEndpoints.FirmAdminRequests.WithStatus(status));
            return result ?? new List<FirmAdminRequestDTO>();
        }

        public Task<int> ApproveAsync(int id) =>
            _api.PutAsync<int>(ApiEndpoints.FirmAdminRequests.Approve(id));

        public Task<bool> RejectAsync(int id, string? reason) =>
            _api.PutAsync<bool>(ApiEndpoints.FirmAdminRequests.Reject(id), new { Reason = Norm(reason) });

        private static string? Norm(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
