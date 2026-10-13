using LTSFrontend.Features.FirmAdminRequests.DTOs;
using LTSFrontend.Features.SuperAdminRequests.Services;
using LTSFrontend.Features.UserJoinRequests.Services;

namespace LTSFrontend.Features.FirmAdminRequests.Services
{
    public sealed class FirmAdminRequestStatusState
    {
        private readonly IFirmAdminRequestService _firmRequests;
        private readonly ISuperAdminRequestService _superAdminRequests;
        private readonly IUserJoinRequestService _joinRequests;
        private Task? _inFlight;

        public FirmAdminRequestStatusState(IFirmAdminRequestService firmRequests, ISuperAdminRequestService superAdminRequests, IUserJoinRequestService joinRequests)
        {
            _firmRequests = firmRequests;
            _superAdminRequests = superAdminRequests;
            _joinRequests = joinRequests;
        }

        public FirmRequestEligibility? Current { get; private set; }
        public bool IsLoading { get; private set; }


        public bool LoadFailed { get; private set; }

        public event Action? OnChange;

        public Task EnsureLoadedAsync() => Current is not null ? Task.CompletedTask : RefreshAsync();

        public Task RefreshAsync()
        {
            if (_inFlight is { IsCompleted: false })
                return _inFlight;

            return _inFlight = LoadAsync();
        }

        public void Reset()
        {
            Current = null;
            LoadFailed = false;
            IsLoading = false;
            OnChange?.Invoke();
        }

        private async Task LoadAsync()
        {
            IsLoading = true;
            LoadFailed = false;
            OnChange?.Invoke();

            FirmAdminRequestDTO? mine = null;
            string? superAdminStatus = null;
            string? joinStatus = null;

            try
            {
                mine = await _firmRequests.GetMineAsync();
            }
            catch (Exception)
            {
                LoadFailed = true;
            }

            try
            {
                superAdminStatus = (await _superAdminRequests.GetMineAsync())?.Status;
            }
            catch (Exception)
            {

            }

            try
            {
                joinStatus = (await _joinRequests.GetMineAsync())?.Status;
            }
            catch (Exception)
            {

            }

            Current = FirmAdminRequestRules.Evaluate(mine, superAdminStatus, joinStatus);
            IsLoading = false;
            OnChange?.Invoke();
        }
    }
}
