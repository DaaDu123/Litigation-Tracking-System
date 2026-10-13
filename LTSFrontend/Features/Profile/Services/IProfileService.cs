using LTSFrontend.Features.Profile.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace LTSFrontend.Features.Profile.Services
{
    public interface IProfileService
    {
        Task<ProfileDTO?> GetMyProfileAsync();
        Task<bool> UpdateAsync(UpdateProfileDTO form, IBrowserFile? profileImage = null);


        Task<ProfileCompletionResultDTO> CompleteFirmAdminProfileAsync(CompleteFirmAdminProfileRequest request);


        Task<ProfileCompletionResultDTO> CompleteFirmUserProfileAsync(CompleteFirmUserProfileRequest request);
    }
}
