namespace LTSFrontend.State
{
    public class ToastMessage
    {
        public Guid Id { get; } = Guid.NewGuid();
        public ToastType Type { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Detail { get; init; }
    }
}
