using System.Collections.Generic;
namespace GameDBWebApi.Entites
{
    /// <summary>
    /// Represents an application user. Stores authentication credentials and
    /// navigation collections for related entities such as library entries and lists.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique username chosen by the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Email address of the user.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// BCrypt hashed password for authentication.
        /// </summary>
        public string HashPassword { get; set; }

        // TOKEN ROTATION PROPERTIES
        /// <summary>
        /// SHA256 fingerprint (Base64) of the current refresh token. Used for lookup only; the token itself is not stored.
        /// </summary>
        public string? RefreshTokenFingerprint { get; set; }

        /// <summary>
        /// Expiration date/time (UTC) of the stored refresh token fingerprint.
        /// </summary>
        public DateTime RefreshTokenExpiryTime { get; set; }

        /// <summary>
        /// UTC time when the user account was created. Populated automatically by AppDbContext.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC time when the user account was last updated. Populated automatically by AppDbContext.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // NAV PROPERTIES

        /// <summary>
        /// Collection of UserGame entries representing the user's library and statuses.
        /// </summary>
        public ICollection<UserGame> UserGames { get; set; }

        /// <summary>
        /// Lists created by the user (favorites, custom collections). May be empty if lists are unused.
        /// </summary>
        public ICollection<UserList> UserLists { get; set; }

    }
}
