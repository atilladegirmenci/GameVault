using System.Collections.Generic;

namespace GameDBWebApi.Entites
{
    public class UserList
    {

        public int Id  { get; set; }
        public string Name { get; set; }

        //foregin key: who owns this list
        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<UserListGame> ListedGames { get; set; }
    }
}
