using LTSFrontend.Features.Users.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace LTSFrontend.Features.Users.Services
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetAllAsync();
        Task<UserDTO?> GetByIdAsync(int id);
        Task<UserDTO?> GetMyProfileAsync();
        Task<int> CreateAsync(CreateUserDTO dto, IBrowserFile? profileImage = null);
        Task<bool> UpdateAsync(CreateUserDTO dto, IBrowserFile? profileImage = null);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> PermanentDeleteAsync(int id);

        // SuperAdmin-only: email-reuse reservation management (see
        // CreateUserCommandHandler for why a deleted user's email is
        // reserved to their original firm until explicitly released).
        Task<List<DeletedUserDTO>> GetDeletedAsync();
        Task<bool> ReleaseAsync(int id);
    }
}
