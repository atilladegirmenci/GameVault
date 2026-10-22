using GameDBWebApi.Entites;

namespace GameDBWebApi.DTOs.Game
{
    public class UpsertGameStatusDto
    {
        public int RawgId { get; set; }

        public string Name { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;
       
        //TODO: i should remove or update this field, because the score is not required for all statuses, only for completed games + it conflicts with reviews.
        public short Score { get; set; }

        public GameStatus Status { get; set; } 
    }
}
