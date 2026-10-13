namespace LTSFrontend.Features.Dashboard.DTOs
{

    public class ChartItem
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }

        public string? Color { get; set; }
    }
}
