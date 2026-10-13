namespace LTSFrontend.State
{









    public class CaseFilterState
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int CourtFilter { get; set; }
        public int StatusFilter { get; set; }
        public string PriorityFilter { get; set; } = string.Empty;
        public string ViewMode { get; set; } = "table";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(SearchTerm) || CourtFilter > 0 || StatusFilter > 0 || !string.IsNullOrWhiteSpace(PriorityFilter);

        public void Reset()
        {
            SearchTerm = string.Empty;
            CourtFilter = 0;
            StatusFilter = 0;
            PriorityFilter = string.Empty;
            PageNumber = 1;
        }
    }
}
