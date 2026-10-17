using GameDBWebApi.Entites;

namespace GameDBWebApi.Repositories
{
    public interface IUserListRepository
    {
        //UserList operations
        Task<UserList?> GetByIdAsync(int listId);

        Task<IEnumerable<UserList>> GetByUserIdAsync(int userId);

        Task<bool> ExistsAsync(int listId);

        Task AddAsync(UserList userList);

        void Update(UserList userList);

        void Delete(UserList userList);

        // UserListGame operations
        Task AddGameAsync(int listId, int gameId);
        Task RemoveGameAsync(int listId, int gameId);
        Task<IEnumerable<UserListGame>> GetGamesByListIdAsync(int listId);
        Task<bool> ContainsGameByRawgIdAsync(int listId, int rawgId);
        Task<bool> ContainsGameAsync(int listId, int gameId);
        Task<int> GetGameCountByListIdAsync(int listId);
    }
}
