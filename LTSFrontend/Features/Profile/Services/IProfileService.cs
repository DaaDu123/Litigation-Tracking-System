using LTSFrontend.Features.Profile.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace LTSFrontend.Features.Profile.Services
{
    public interface IProfileService
    {
        Task<ProfileDTO?> GetMyProfileAsync();
        Task<bool> UpdateAsync(UpdateProfileDTO form, IBrowserFile? profileImage = null);

        /// <summary>Mandatory one-time step for a newly approved Firm Admin - see ProfileCompletionGate.</summary>
        Task<ProfileCompletionResultDTO> CompleteFirmAdminProfileAsync(CompleteFirmAdminProfileRequest request);

        /// <summary>Mandatory one-time step for a newly registered Firm User - see ProfileCompletionGate.</summary>
        Task<ProfileCompletionResultDTO> CompleteFirmUserProfileAsync(CompleteFirmUserProfileRequest request);
    }
}
