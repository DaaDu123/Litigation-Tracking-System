namespace LTSFrontend.Features.Dashboard.DTOs
{
    /// <summary>One bar in a <c>ChartWidget</c> (e.g. a case status or priority bucket).</summary>
    public class ChartItem
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        /// <summary>CSS color for the bar, e.g. "#1b4fd6". Falls back to the widget's default accent when null.</summary>
        public string? Color { get; set; }
    }
}
