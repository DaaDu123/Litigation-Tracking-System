using LTSFrontend.Core.Exceptions;
using LTSFrontend.Core.Http;
using LTSFrontend.Features.SuperAdminRequests.DTOs;

namespace LTSFrontend.Features.SuperAdminRequests.Services
{
    public class SuperAdminRequestService : ISuperAdminRequestService
    {
        private readonly ApiClient _api;

        public SuperAdminRequestService(ApiClient api)
        {
            _api = api;
        }

        public async Task<SuperAdminRequestDTO?> GetMineAsync()
        {
            try
            {
                return await _api.GetAsync<SuperAdminRequestDTO?>(ApiEndpoints.SuperAdminRequests.Mine);
            }
            catch (ApiException ex) when (ex.StatusCode is 404 or 405 or 501)
            {
                return null;
            }
        }
    }
}
