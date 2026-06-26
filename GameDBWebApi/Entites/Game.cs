using System.Collections.Generic;

namespace GameDBWebApi.Entites
{
    public class Game
    {

        public int Id { get; set; }
        public int RawgId { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? ReleaseDate { get; set; }

        //NAV PROPERTIES

        public ICollection<UserGame> UserGames { get; set; }

        // dont want this property to be used for now, as it is not needed in the current implementation
        // public ICollection<UserList> UserLists { get; set; }
    }
}
