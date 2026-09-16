namespace LTSFrontend.State
{
    /// <summary>
    /// Scoped (one instance per user circuit), general-purpose holder for
    /// cross-cutting UI state that doesn't belong to any single page - e.g. a
    /// global "background operation in progress" indicator for long-running
    /// actions like Super Admin's firm data export/migration (SRS FR-4/FR-5)
    /// that a person might trigger and then navigate away from before it
    /// finishes. Session identity lives in UserSessionState and toast
    /// messages live in ToastService - this is only for state that doesn't
    /// fit either of those.
    /// </summary>
    public class AppState
    {
        public bool IsBusy { get; private set; }
        public string? BusyMessage { get; private set; }

        public event Action? OnChange;

        /// <summary>Marks a long-running, app-wide operation as in progress (e.g. shown as a top-bar indicator in MainLayout).</summary>
        public void SetBusy(string? message = null)
        {
            IsBusy = true;
            BusyMessage = message;
            NotifyStateChanged();
        }

        public void ClearBusy()
        {
            IsBusy = false;
            BusyMessage = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
