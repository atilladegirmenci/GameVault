
using GameDBWebApi.Data;
using GameDBWebApi.DTOs.List;
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

        public async Task<IEnumerable<UserListResponseDto>> GetSummariesByUserIdAsync(
            int userId,
            int previewCount)
        {
            return await _context.UserLists
                .AsNoTracking()
                .Where(ul => ul.UserId == userId)
                .OrderBy(ul => ul.Type == UserListType.Custom ? 1 : 0)
                .ThenBy(ul => ul.Type)
                .ThenBy(ul => ul.Id)
                .Select(ul => new UserListResponseDto
                {
                    Id = ul.Id,
                    ListTitle = ul.ListTitle,
                    UserId = ul.UserId,
                    Type = ul.Type,
                    GameCount = ul.ListedGames.Count,
                    PreviewImageUrls = ul.ListedGames
                        .Where(ulg => ulg.Game.ImageUrl != null)
                        .OrderByDescending(ulg => ulg.AddedDate)
                        .ThenBy(ulg => ulg.GameId)
                        .Select(ulg => ulg.Game.ImageUrl!)
                        .Take(previewCount)
                        .ToList()
                })
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
                AddedDate = DateTime.UtcNow,
                Position = (await _context.UserListGames
                    .Where(ulg => ulg.UserListId == listId)
                    .MaxAsync(ulg => (int?)ulg.Position) ?? -1) + 1
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
                .OrderBy(ulg => ulg.Position)
                .ThenBy(ulg => ulg.GameId)
                .Include(ulg => ulg.Game)
                .ToListAsync();
        }

        public async Task<bool> ContainsGameByCatalogIdAsync(int listId, int catalogId)
        {
            return await _context.UserListGames
                .AnyAsync(ulg =>
                    ulg.UserListId == listId &&
                    ulg.Game.CatalogId == catalogId);
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

        public async Task ReorderGamesAsync(int listId, IReadOnlyList<int> orderedGameIds)
        {
            var entries = await _context.UserListGames
                .Where(ulg => ulg.UserListId == listId)
                .ToListAsync();
            var positions = orderedGameIds
                .Select((gameId, position) => new { gameId, position })
                .ToDictionary(item => item.gameId, item => item.position);

            foreach (var entry in entries)
                entry.Position = positions[entry.GameId];
        }

        public async Task EnsureDefaultListsAsync(int userId)
        {
            var existingTypes = await _context.UserLists
                .Where(list => list.UserId == userId && list.Type != UserListType.Custom)
                .Select(list => list.Type)
                .ToListAsync();
            var existing = existingTypes.ToHashSet();
            var now = DateTime.UtcNow;

            foreach (var (type, title) in DefaultListTitles)
            {
                if (!existing.Contains(type))
                {
                    await _context.UserLists.AddAsync(new UserList
                    {
                        UserId = userId,
                        Type = type,
                        ListTitle = title,
                        CreatedAt = now
                    });
                }
            }
        }

        public async Task SyncGameStatusListAsync(int userId, int gameId, GameStatus status)
        {
            await EnsureDefaultListsAsync(userId);

            var targetType = status switch
            {
                GameStatus.PlansToPlay => UserListType.PlansToPlay,
                GameStatus.Playing => UserListType.CurrentlyPlaying,
                GameStatus.Finished => UserListType.Finished,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown game status.")
            };

            var defaultLists = await _context.UserLists
                .Where(list => list.UserId == userId && list.Type != UserListType.Custom)
                .ToListAsync();
            var trackedDefaults = _context.UserLists.Local
                .Where(list => list.UserId == userId && list.Type != UserListType.Custom)
                .Where(list => defaultLists.All(existing => existing.Id != list.Id));
            defaultLists.AddRange(trackedDefaults);
            var targetList = defaultLists.Single(list => list.Type == targetType);
            var defaultListIds = defaultLists.Select(list => list.Id).ToArray();
            var memberships = await _context.UserListGames
                .Where(entry => entry.GameId == gameId &&
                                defaultListIds.Contains(entry.UserListId))
                .ToListAsync();
            var targetMembership = memberships.FirstOrDefault(entry => entry.UserListId == targetList.Id);

            _context.UserListGames.RemoveRange(memberships.Where(entry => entry.UserListId != targetList.Id));

            if (targetMembership is null)
            {
                var lastPosition = await _context.UserListGames
                    .Where(entry => entry.UserListId == targetList.Id)
                    .MaxAsync(entry => (int?)entry.Position) ?? -1;
                var membership = new UserListGame
                {
                    GameId = gameId,
                    AddedDate = DateTime.UtcNow,
                    Position = lastPosition + 1
                };
                if (_context.Entry(targetList).State == EntityState.Added)
                    membership.UserList = targetList;
                else
                    membership.UserListId = targetList.Id;
                await _context.UserListGames.AddAsync(membership);
            }
        }

        public async Task RemoveGameFromStatusListsAsync(int userId, int gameId)
        {
            var defaultListIds = _context.UserLists
                .Where(list => list.UserId == userId && list.Type != UserListType.Custom)
                .Select(list => list.Id);
            var memberships = await _context.UserListGames
                .Where(entry => entry.GameId == gameId && defaultListIds.Contains(entry.UserListId))
                .ToListAsync();
            _context.UserListGames.RemoveRange(memberships);
        }

        private static readonly (UserListType Type, string Title)[] DefaultListTitles =
        [
            (UserListType.PlansToPlay, "Plans to Play"),
            (UserListType.CurrentlyPlaying, "Currently Playing"),
            (UserListType.Finished, "Finished")
        ];
    }
}
