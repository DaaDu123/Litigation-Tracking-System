using LTSFrontend.Features.Profile.DTOs;
using LTSFrontend.Features.Profile.Services;

namespace LTSFrontend.State
{
    /// <summary>
    /// Scoped cache of the current user's own profile-completion status, so
    /// ProfileCompletionGate doesn't refetch on every navigation within the
    /// same circuit. Call MarkCompleted() right after a successful
    /// CompleteFirmAdminProfile/CompleteFirmUserProfile call instead of a
    /// full RefreshAsync() round-trip.
    /// </summary>
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

        /// <summary>Call on logout so the next login doesn't see a stale cached profile.</summary>
        public void Reset()
        {
            Profile = null;
            IsLoaded = false;
            OnChange?.Invoke();
        }
    }
}
