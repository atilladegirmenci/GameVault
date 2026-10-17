namespace GameDBWebApi.DTOs.List
{
    public class UserListGameResponseDto
    {
        public int GameId { get; set; }

        public int RawgId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public DateTime AddedDate { get; set; }
    }
}
