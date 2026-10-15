using System.Collections.Generic;
namespace GameDBWebApi.Entites
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string HashPassword { get; set; }

        // TOKEN ROTATION PROPERTIES
        // Store only the fingerprint (SHA256) of the refresh token for lookup
        public string? RefreshTokenFingerprint { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        //NAV PROPERTIES

        public ICollection<UserGame> UserGames { get; set; }
        public ICollection<UserList> UserLists { get; set; }

      
    }
}
