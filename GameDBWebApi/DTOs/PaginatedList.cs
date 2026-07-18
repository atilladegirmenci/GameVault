namespace GameDBWebApi.DTOs
{
    // Generic class to represent a paginated list of items
    public class PaginatedList<T>: List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        //for frontend, useful to know if there is a previous or next page
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;

            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            // add the incoming items to the list
            this.AddRange(items);
        }
    }
}
