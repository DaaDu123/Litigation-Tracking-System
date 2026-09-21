using LTSFrontend.Features.Users.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace LTSFrontend.Features.Users.Services
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetAllAsync(string? search = null);
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

        // Firm Admin: block/unblock/remove a firm user, and role changes.
        Task<bool> BlockAsync(int id, string reason);
        Task<bool> UnblockAsync(int id);
        Task<bool> RemoveAsync(int id, string reason);
        Task<List<BlockedFirmUserDTO>> GetBlockedAsync();
        Task<bool> ChangeRoleAsync(int id, int newRoleId);

        // Firm Admin availability (Active/Inactive).
        Task<bool> SetMyAvailabilityAsync(SetAvailabilityRequest request);
        Task<FirmAdminAvailabilityDTO?> GetFirmAdminAvailabilityAsync();
    }
}
