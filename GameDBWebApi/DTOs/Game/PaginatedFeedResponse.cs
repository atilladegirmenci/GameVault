namespace GameDBWebApi.DTOs.Game
{
    public class PaginatedFeedResponse
    {
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<GameFeedDto> Games { get; set; } = new();
    }
}
