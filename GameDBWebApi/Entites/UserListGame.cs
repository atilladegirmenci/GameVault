namespace GameDBWebApi.Entites
{
    //junction table for many-to-many relationship between UserList and Game
    public class UserListGame
    {
        //Foreign key to UserList and Game
        public int UserListId { get; set; }
        public UserList UserList { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }

        public DateTime AddedDate { get; set; }
    }
}
