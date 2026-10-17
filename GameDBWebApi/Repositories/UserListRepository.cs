
using GameDBWebApi.Data;
using GameDBWebApi.Entites;
using Microsoft.EntityFrameworkCore;

namespace GameDBWebApi.Repositories
{
    public class UserListRepository : IUserListRepository
    {
        private readonly AppDbContext _context;

        public UserListRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserList?> GetByIdAsync(int listId)
        {
            return await _context.UserLists
                .FirstOrDefaultAsync(ul => ul.Id == listId);
        }

        public async Task<bool> ExistsAsync(int listId)
        {
            return await _context.UserLists
                .AnyAsync(ul => ul.Id == listId);
        }

        // ListedGames is included so the service can calculate GameCount.
        public async Task<IEnumerable<UserList>> GetByUserIdAsync(int userId)
        {
            return await _context.UserLists
                .Where(ul => ul.UserId == userId)
                .Include(ul => ul.ListedGames)
                .ToListAsync();
        }

        public async Task AddAsync(UserList userList)
        {
            await _context.UserLists.AddAsync(userList);
        }

        public void Update(UserList userList)
        {
            _context.UserLists.Update(userList);
        }

        public void Delete(UserList userList)
        {
            _context.UserLists.Remove(userList);
        }

        // Adds a game to a list.
        public async Task AddGameAsync(int listId, int gameId)
        {
            var userListGame = new UserListGame
            {
                UserListId = listId,
                GameId = gameId,
                AddedDate = DateTime.UtcNow
            };

            await _context.UserListGames.AddAsync(userListGame);
        }

        // Removes a game from a list.
        public async Task RemoveGameAsync(int listId, int gameId)
        {
            var userListGame = await _context.UserListGames
                .FirstOrDefaultAsync(ulg =>
                    ulg.UserListId == listId &&
                    ulg.GameId == gameId);

            if (userListGame is not null)
            {
                _context.UserListGames.Remove(userListGame);
            }
        }

        // Retrieves all games in a list, including their game details.
        public async Task<IEnumerable<UserListGame>> GetGamesByListIdAsync(
            int listId)
        {
            return await _context.UserListGames
                .Where(ulg => ulg.UserListId == listId)
                .Include(ulg => ulg.Game)
                .ToListAsync();
        }

        public async Task<bool> ContainsGameByRawgIdAsync(int listId, int rawgId)
        {
            return await _context.UserListGames
                .AnyAsync(ulg =>
                    ulg.UserListId == listId &&
                    ulg.Game.RawgId == rawgId);
        }

        public async Task<bool> ContainsGameAsync(int listId, int gameId)
        {
            return await _context.UserListGames
                .AnyAsync(ulg => ulg.UserListId == listId && ulg.GameId == gameId);
        }

        public async Task<int> GetGameCountByListIdAsync(int listId)
        {
            return await _context.UserListGames
                .CountAsync(ulg => ulg.UserListId == listId);
        }
    }
}
