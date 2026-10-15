using System.Collections.Generic;

namespace GameDBWebApi.Entites
{
    /// <summary>
    /// Represents a game in the local database. Many fields are synchronized from the RAWG API.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Primary key for the Game entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The identifier of the game from the RAWG external API.
        /// </summary>
        public int RawgId { get; set; }

        /// <summary>
        /// Human-readable title of the game.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL to the game's cover or background image.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Official release date if known. Nullable when not available.
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Aggregate score (e.g. Metacritic) stored as a small integer.
        /// </summary>
        public short Score { get; set; }

        // NAV PROPERTIES

        /// <summary>
        /// Collection of user-specific records that reference this game (user library/status).
        /// </summary>
        public ICollection<UserGame> UserGames { get; set; }

        // The relationship to UserList is intentionally not used in the current implementation.
        // public ICollection<UserList> UserLists { get; set; }
    }
}
