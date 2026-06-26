using System.Collections.Generic;
namespace GameDBWebApi.Entites
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string HashPassword { get; set; }

        //NAV PROPERTIES

        public ICollection<UserGame> UserGames { get; set; }
        public ICollection<UserList> UserLists { get; set; }
    }
}
