namespace GameDBWebApi.DTOs
{
    // Generic object response for paginated API endpoints.
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public int TotalCount { get; }

        //for frontend, useful to know if there is a previous or next page
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;

            TotalPages = pageSize > 0
                ? (int)Math.Ceiling(count / (double)pageSize)
                : 0;

            Items = items;
        }
    }
}
