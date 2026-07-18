using GameDBWebApi.Entites;

namespace GameDBWebApi.DTOs.Game
{
    public class GameCardDto
    {
        public int RawgId { get; set; }
        public string Title { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public short Score { get; set; }

        //user related properties (null if not in db or user not logged in)
        public int GameId { get; set; }
        public GameStatus? Status { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
