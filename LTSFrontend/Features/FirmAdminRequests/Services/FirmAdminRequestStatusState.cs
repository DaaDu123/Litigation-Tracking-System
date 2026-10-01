using LTSFrontend.Features.FirmAdminRequests.DTOs;
using LTSFrontend.Features.SuperAdminRequests.Services;

namespace LTSFrontend.Features.FirmAdminRequests.Services
{
    public sealed class FirmAdminRequestStatusState
    {
        private readonly IFirmAdminRequestService _firmRequests;
        private readonly ISuperAdminRequestService _superAdminRequests;
        private Task? _inFlight;

        public FirmAdminRequestStatusState(IFirmAdminRequestService firmRequests, ISuperAdminRequestService superAdminRequests)
        {
            _firmRequests = firmRequests;
            _superAdminRequests = superAdminRequests;
        }

        public FirmRequestEligibility? Current { get; private set; }
        public bool IsLoading { get; private set; }

        /// <summary>True when the status calls failed; the UI then offers the form and lets the API be the judge.</summary>
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
                // Non-fatal: the backend enforces the mutual restriction on submit anyway.
            }

            Current = FirmAdminRequestRules.Evaluate(mine, superAdminStatus);
            IsLoading = false;
            OnChange?.Invoke();
        }
    }
}
