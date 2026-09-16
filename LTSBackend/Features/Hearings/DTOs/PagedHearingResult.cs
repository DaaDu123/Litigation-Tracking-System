using System.Collections.Generic;

namespace LTSBackend.Features.Hearings.DTOs
{
    public class PagedHearingResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}