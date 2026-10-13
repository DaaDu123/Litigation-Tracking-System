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




        Task<List<DeletedUserDTO>> GetDeletedAsync();
        Task<bool> ReleaseAsync(int id);


        Task<bool> BlockAsync(int id, string reason);
        Task<bool> UnblockAsync(int id);
        Task<bool> RemoveAsync(int id, string reason);
        Task<List<BlockedFirmUserDTO>> GetBlockedAsync();
        Task<bool> ChangeRoleAsync(int id, int newRoleId);


        Task<bool> SetMyAvailabilityAsync(SetAvailabilityRequest request);
        Task<FirmAdminAvailabilityDTO?> GetFirmAdminAvailabilityAsync();
    }
}
