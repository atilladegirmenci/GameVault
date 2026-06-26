namespace GameDBWebApi.Entites
{
    public class UserGame
    {
        //Foreign key to User and Game
        public int UserId { get; set; }
        public User User { get; set; }
       
        public int GameId { get; set; }
        public Game Game { get; set; }

        public GameStatus Status { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
