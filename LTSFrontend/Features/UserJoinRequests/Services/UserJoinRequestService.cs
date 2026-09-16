using LTSFrontend.Core.Http;
using LTSFrontend.Features.UserJoinRequests.DTOs;

namespace LTSFrontend.Features.UserJoinRequests.Services
{
    public class UserJoinRequestService : IUserJoinRequestService
    {
        private readonly ApiClient _api;

        public UserJoinRequestService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<JoinableFirmDTO>> GetJoinableFirmsAsync()
        {
            var result = await _api.GetAsync<List<JoinableFirmDTO>>(ApiEndpoints.UserJoinRequests.Firms);
            return result ?? new List<JoinableFirmDTO>();
        }

        public Task<int> SubmitAsync(SubmitUserJoinRequest request) =>
            _api.PostAsync<int>(ApiEndpoints.UserJoinRequests.Base_, new
            {
                request.FirmID,
                request.FullName,
                request.Email,
                Password = request.Password,
                Phone = Norm(request.Phone),
                Department = Norm(request.Department),
                request.RequestedRoleID
            });

        public async Task<List<UserJoinRequestDTO>> GetAllAsync(string? status = null)
        {
            var result = await _api.GetAsync<List<UserJoinRequestDTO>>(ApiEndpoints.UserJoinRequests.WithStatus(status));
            return result ?? new List<UserJoinRequestDTO>();
        }

        public Task<int> ApproveAsync(int id)
        {
            return _api.PutAsync<int>(ApiEndpoints.UserJoinRequests.Approve(id));
        }

        public Task<bool> RejectAsync(int id, string? reason)
        {
            return _api.PutAsync<bool>(ApiEndpoints.UserJoinRequests.Reject(id), new { Reason = Norm(reason) });
        }

        private static string? Norm(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
