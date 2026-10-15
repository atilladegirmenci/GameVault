namespace GameDBWebApi.Entites
{
    /// <summary>
    /// Junction table entity representing the many-to-many relationship between UserList and Game.
    /// Stores the association and metadata such as when the game was added to the list.
    /// </summary>
    public class UserListGame
    {
        /// <summary>
        /// Foreign key to the owning UserList.
        /// </summary>
        public int UserListId { get; set; }

        /// <summary>
        /// Navigation to the parent UserList.
        /// </summary>
        public UserList UserList { get; set; }

        /// <summary>
        /// Foreign key to the Game entity.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Navigation to the Game entity.
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// UTC time when the game was added to the list.
        /// </summary>
        public DateTime AddedDate { get; set; }
    }
}
