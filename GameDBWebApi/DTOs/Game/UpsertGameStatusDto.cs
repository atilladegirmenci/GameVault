using GameDBWebApi.Entites;

namespace GameDBWebApi.DTOs.Game
{
    public class UpsertGameStatusDto
    {
        public int RawgId { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public short Score { get; set; }
        // public DateTime? ReleaseDate { get; set; }

        public GameStatus Status { get; set; }
    }
}
