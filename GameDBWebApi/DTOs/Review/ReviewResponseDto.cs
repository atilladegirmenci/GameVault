namespace GameDBWebApi.DTOs.Review
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }

        public int GameId { get; set; }

        public int RawgId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public short Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
