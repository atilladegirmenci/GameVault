namespace GameDBWebApi.DTOs.Game
{
    public class GameFeedDto
    {
        public int RawgId { get; set; }
        public string Title { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public short Score { get; set; }
    }
}
