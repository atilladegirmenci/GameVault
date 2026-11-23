using GameDBWebApi.Entites;
using GameDBWebApi.DTOs.List;

namespace GameDBWebApi.Repositories
{
    public interface IUserListRepository
    {
        //UserList operations
        Task<UserList?> GetByIdAsync(int listId);

        Task<IEnumerable<UserListResponseDto>> GetSummariesByUserIdAsync(int userId, int previewCount);

        Task<bool> ExistsAsync(int listId);

        Task AddAsync(UserList userList);

        void Update(UserList userList);

        void Delete(UserList userList);

        // UserListGame operations
        Task AddGameAsync(int listId, int gameId);
        Task RemoveGameAsync(int listId, int gameId);
        Task<IEnumerable<UserListGame>> GetGamesByListIdAsync(int listId);
        Task<bool> ContainsGameByCatalogIdAsync(int listId, int catalogId);
        Task<bool> ContainsGameAsync(int listId, int gameId);
        Task<int> GetGameCountByListIdAsync(int listId);
        Task ReorderGamesAsync(int listId, IReadOnlyList<int> orderedGameIds);
        Task EnsureDefaultListsAsync(int userId);
        Task SyncGameStatusListAsync(int userId, int gameId, GameStatus status);
        Task RemoveGameFromStatusListsAsync(int userId, int gameId);
    }
}
