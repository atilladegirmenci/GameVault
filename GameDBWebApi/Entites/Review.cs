namespace GameDBWebApi.Entites
{
    public class Review
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int UserId { get; set; }

        public short Rating { get; set; }
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        // Nav Properties
        public Game Game { get; set; }
        public User User { get; set; }
    }
}

