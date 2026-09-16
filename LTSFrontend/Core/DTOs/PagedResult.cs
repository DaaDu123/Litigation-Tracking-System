namespace LTSFrontend.Core.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages
        {
            get
            {
                if (PageSize > 0)
                {
                    return (int)Math.Ceiling((double)TotalRecords / PageSize);
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
