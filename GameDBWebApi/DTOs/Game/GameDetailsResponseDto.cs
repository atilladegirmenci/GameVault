using GameDBWebApi.Entites;

namespace GameDBWebApi.DTOs.Game
{
    public class GameDetailsResponseDto
    {
        public int RawgId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } 
        public string ImageUrl { get; set; }
        public DateTime? Released { get; set; }
        public short Score { get; set; }
        // one game may have multiple genres, developers, tags, and publishers, so we use List<string> to represent them
        public List<string> Genres { get; set; } = new();
        public List<string> Developers { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public List<string> Publishers { get; set; } = new();

        // data related to the user, if the user is logged in
        public bool IsInMyLibrary { get; set; }
        public GameStatus? MyStatus { get; set; } 
    }
}
