namespace GameDBWebApi.Entites
{
    /// <summary>
    /// Represents the relationship between a User and a Game. Stores the user's
    /// status for the game (e.g. playing, finished) and the last update time.
    /// </summary>
    public class UserGame
    {
        /// <summary>
        /// Foreign key to the owning User.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Navigation to the user who owns this entry.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Foreign key to the referenced Game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Navigation to the referenced Game entity.
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// The user's current status for this game (planned, playing, finished).
        /// </summary>
        public GameStatus Status { get; set; }

        /// <summary>
        /// UTC time when this relationship was last updated.
        /// </summary>
        public DateTime UpdatedDate { get; set; }
    }
}
