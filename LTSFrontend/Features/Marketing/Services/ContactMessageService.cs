using LTSFrontend.Core.Http;
using LTSFrontend.Features.Marketing.DTOs;

namespace LTSFrontend.Features.Marketing.Services
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly ApiClient _api;

        public ContactMessageService(ApiClient api)
        {
            _api = api;
        }

        public Task SubmitAsync(SubmitContactMessage request) =>
            _api.PostAsync<bool>(ApiEndpoints.ContactMessages.Base_, new
            {
                request.Name,
                request.Email,
                Phone = Norm(request.Phone),
                request.Message
            });

        private static string? Norm(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
