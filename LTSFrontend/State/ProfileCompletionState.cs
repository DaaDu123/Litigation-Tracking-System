using LTSFrontend.Features.Profile.DTOs;
using LTSFrontend.Features.Profile.Services;

namespace LTSFrontend.State
{







    public class ProfileCompletionState(IProfileService _profileService)
    {
        public ProfileDTO? Profile { get; private set; }
        public bool IsLoaded { get; private set; }

        public event Action? OnChange;

        public async Task<ProfileDTO?> EnsureLoadedAsync()
        {
            if (IsLoaded)
                return Profile;

            await RefreshAsync();
            return Profile;
        }

        public async Task RefreshAsync()
        {
            Profile = await _profileService.GetMyProfileAsync();
            IsLoaded = true;
            OnChange?.Invoke();
        }

        public void MarkCompleted()
        {
            if (Profile != null)
            {
                Profile.IsProfileCompleted = true;
                OnChange?.Invoke();
            }
        }


        public void Reset()
        {
            Profile = null;
            IsLoaded = false;
            OnChange?.Invoke();
        }
    }
}
