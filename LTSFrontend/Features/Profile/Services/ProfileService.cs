using LTSFrontend.Core.Http;
using LTSFrontend.Features.Profile.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace LTSFrontend.Features.Profile.Services
{
    public class ProfileService : IProfileService
    {
        private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB, mirrors backend rule
        private readonly ApiClient _api;

        public ProfileService(ApiClient api)
        {
            _api = api;
        }

        public Task<ProfileDTO?> GetMyProfileAsync() =>
            _api.GetAsync<ProfileDTO>(ApiEndpoints.Profile.Me);

        public async Task<bool> UpdateAsync(UpdateProfileDTO form, IBrowserFile? profileImage = null)
        {
            using var content = new MultipartFormDataContent
            {
                { new StringContent(form.FullName), "FullName" }
            };

            if (!string.IsNullOrWhiteSpace(form.Phone))
                content.Add(new StringContent(form.Phone), "Phone");

            if (!string.IsNullOrWhiteSpace(form.Department))
                content.Add(new StringContent(form.Department), "Department");

            if (profileImage != null)
            {
                if (profileImage.Size > MaxImageBytes)
                    throw new InvalidOperationException("Profile image cannot exceed 5 MB.");

                var stream = profileImage.OpenReadStream(MaxImageBytes);
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(profileImage.ContentType);
                content.Add(fileContent, "ProfileImage", profileImage.Name);
            }

            return await _api.PutFormAsync<bool>(ApiEndpoints.Profile.Me, content);
        }
    }
}
