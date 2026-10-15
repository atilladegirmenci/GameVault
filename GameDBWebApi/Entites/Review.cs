namespace GameDBWebApi.Entites
{
    /// <summary>
    /// Represents a user review for a specific game. Reviews include a numeric rating and
    /// an optional textual comment. CreatedAt stores the UTC timestamp when the review was created.
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Primary key for the review.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the Game entity being reviewed.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Foreign key to the User entity who authored the review.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Numeric rating given by the user (e.g. 1-10 or 1-5 depending on UI contract).
        /// </summary>
        public short Rating { get; set; }

        /// <summary>
        /// Textual comment provided by the user.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// UTC time when the review was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // Nav Properties

        /// <summary>
        /// Navigation to the reviewed Game entity.
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Navigation to the User who created the review.
        /// </summary>
        public User User { get; set; }
    }
}

