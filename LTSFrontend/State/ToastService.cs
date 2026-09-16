namespace LTSFrontend.State
{
    /// <summary>
    /// Scoped (one per user circuit) pub/sub service that powers the
    /// app-wide toast notification stack. Any page/service can call
    /// Success/Error/Warning/Info; ToastNotification.razor (mounted once
    /// in MainLayout) subscribes to OnChange and renders the stack.
    /// </summary>
    public class ToastService
    {
        private readonly List<ToastMessage> _toasts = new();
        public IReadOnlyList<ToastMessage> Toasts => _toasts;

        public event Action? OnChange;

        public void Success(string title, string? detail = null) => Show(ToastType.Success, title, detail);
        public void Error(string title, string? detail = null) => Show(ToastType.Error, title, detail);
        public void Warning(string title, string? detail = null) => Show(ToastType.Warning, title, detail);
        public void Info(string title, string? detail = null) => Show(ToastType.Info, title, detail);

        public void Show(ToastType type, string title, string? detail = null)
        {
            var toast = new ToastMessage { Type = type, Title = title, Detail = detail };
            _toasts.Add(toast);
            OnChange?.Invoke();

            var lifespan = type == ToastType.Error ? TimeSpan.FromSeconds(7) : TimeSpan.FromSeconds(4.5);
            _ = AutoDismissAsync(toast.Id, lifespan);
        }

        private async Task AutoDismissAsync(Guid id, TimeSpan delay)
        {
            await Task.Delay(delay);
            Dismiss(id);
        }

        public void Dismiss(Guid id)
        {
            _toasts.RemoveAll(t => t.Id == id);
            OnChange?.Invoke();
        }
    }
}
