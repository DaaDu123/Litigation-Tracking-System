namespace LTSFrontend.State
{
    /// <summary>
    /// Scoped (one instance per user circuit) holder for the Case list's
    /// search/filter selections, mirroring the fields CaseList.razor binds
    /// to CaseFilterPanel (SearchTerm, CourtFilter, StatusFilter,
    /// PriorityFilter, PageSize, ViewMode). Lets a person open a case from
    /// the list, come back, and land on the same filtered/paged view
    /// instead of losing their search - CaseList reads/writes this on
    /// init/dispose instead of only keeping the values in local fields.
    /// </summary>
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
