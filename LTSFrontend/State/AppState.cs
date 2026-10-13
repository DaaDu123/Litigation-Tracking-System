namespace LTSFrontend.State
{










    public class AppState
    {
        public bool IsBusy { get; private set; }
        public string? BusyMessage { get; private set; }

        public event Action? OnChange;


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
