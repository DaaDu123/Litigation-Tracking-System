using LTSFrontend.Core.Http;
using LTSFrontend.Features.Users.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace LTSFrontend.Features.Users.Services
{
    public class UserService : IUserService
    {
        private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB, mirrors backend rule
        private readonly ApiClient _api;

        public UserService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<UserDTO>> GetAllAsync()
        {
            var result = await _api.GetAsync<List<UserDTO>>(ApiEndpoints.Users.Base_);
            return result ?? new List<UserDTO>();
        }

        public Task<UserDTO?> GetByIdAsync(int id) =>
            _api.GetAsync<UserDTO>(ApiEndpoints.Users.ById(id));

        public Task<UserDTO?> GetMyProfileAsync() =>
            _api.GetAsync<UserDTO>(ApiEndpoints.Users.MyProfile);

        public async Task<int> CreateAsync(CreateUserDTO dto, IBrowserFile? profileImage = null)
        {
            using var form = await BuildFormAsync(dto, profileImage, includePassword: true);
            return await _api.PostFormAsync<int>(ApiEndpoints.Users.Base_, form);
        }

        public async Task<bool> UpdateAsync(CreateUserDTO dto, IBrowserFile? profileImage = null)
        {
            if (dto.UserID is null)
                throw new InvalidOperationException("UserID is required to update a user.");

            using var form = await BuildFormAsync(dto, profileImage, includePassword: false);
            form.Add(new StringContent(dto.UserID.Value.ToString()), "UserID");
            form.Add(new StringContent(dto.IsActive.ToString()), "IsActive");

            return await _api.PutFormAsync<bool>(ApiEndpoints.Users.ById(dto.UserID.Value), form);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _api.DeleteAsync<bool>(ApiEndpoints.Users.ById(id));
        }

        public async Task<bool> ActivateAsync(int id)
        {
            return await _api.PutAsync<bool>(ApiEndpoints.Users.Activate(id), null);
        }

        public async Task<bool> PermanentDeleteAsync(int id)
        {
            return await _api.DeleteAsync<bool>(ApiEndpoints.Users.PermanentDelete(id));
        }

        public async Task<List<DeletedUserDTO>> GetDeletedAsync()
        {
            var result = await _api.GetAsync<List<DeletedUserDTO>>(ApiEndpoints.Users.Deleted);
            return result ?? new List<DeletedUserDTO>();
        }

        public async Task<bool> ReleaseAsync(int id)
        {
            return await _api.PutAsync<bool>(ApiEndpoints.Users.Release(id), null);
        }

        private static async Task<MultipartFormDataContent> BuildFormAsync(
            CreateUserDTO dto, IBrowserFile? profileImage, bool includePassword)
        {
            var form = new MultipartFormDataContent
            {
                { new StringContent(dto.FullName ?? string.Empty), "FullName" },
                { new StringContent(dto.Email ?? string.Empty), "Email" }
            };

            if (includePassword && !string.IsNullOrWhiteSpace(dto.Password))
                form.Add(new StringContent(dto.Password), "Password");

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                form.Add(new StringContent(dto.Phone), "Phone");

            if (!string.IsNullOrWhiteSpace(dto.Department))
                form.Add(new StringContent(dto.Department), "Department");

            if (dto.RoleID.HasValue)
                form.Add(new StringContent(dto.RoleID.Value.ToString()), "RoleID");

            if (profileImage != null)
            {
                if (profileImage.Size > MaxImageBytes)
                    throw new InvalidOperationException("Profile image cannot exceed 5 MB.");

                var stream = profileImage.OpenReadStream(MaxImageBytes);
                var content = new StreamContent(stream);
                content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(profileImage.ContentType);
                form.Add(content, "ProfileImage", profileImage.Name);
            }

            return form;
        }
    }
}