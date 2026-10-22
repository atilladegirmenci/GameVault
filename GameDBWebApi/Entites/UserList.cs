using System.Collections.Generic;

namespace GameDBWebApi.Entites
{
    /// <summary>
    /// Represents a custom list created by a user (for example: favorites, wish list, custom collections).
    /// Each list belongs to a single user and contains many games via the junction UserListGame.
    /// </summary>
    public class UserList
    {
        /// <summary>
        /// Primary key of the list.
        /// </summary>
        public int Id  { get; set; }

        /// <summary>
        /// User-defined name for the list.
        /// </summary>
        public string ListTitle { get; set; }

        // foreign key: who owns this list
        /// <summary>
        /// Id of the user who owns this list.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Navigation to the owner user.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Games contained in this list via the junction entity.
        /// </summary>
        public ICollection<UserListGame> ListedGames { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
