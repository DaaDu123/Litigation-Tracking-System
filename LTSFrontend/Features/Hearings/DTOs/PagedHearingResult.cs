namespace LTSFrontend.Features.Hearings.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.Hearings.DTOs.PagedHearingResult&lt;T&gt; (uses TotalCount, not TotalRecords).</summary>
    public class PagedHearingResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }
}
