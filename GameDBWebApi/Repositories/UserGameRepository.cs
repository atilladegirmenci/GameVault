using GameDBWebApi.Data;
using GameDBWebApi.DTOs;
using GameDBWebApi.Entites;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace GameDBWebApi.Repositories
{
    public class UserGameRepository : IUserGameRepository
    {
        private readonly AppDbContext _context;
        public UserGameRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task AddAsync(UserGame userGame)
        {
            _context.UserGames.Add(userGame);
        }

        public async Task<UserGame?> GetByUserIDAndGameIdAsync(int userId, int gameId)
        {
           return await _context.UserGames
                            .Where(ug => ug.UserId == userId && ug.GameId == gameId)
                            .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(UserGame userGame)
        {
            _context.UserGames.Update(userGame);
        }

        public async Task<PaginatedList<UserGame>> GetByUserIdAsync(int userId, int page, int pageSize)
        {
            // iqueryable
            var query = _context.UserGames
                                .Include(ug => ug.Game) // related Game entity
                                .Where(ug => ug.UserId == userId)
                                .OrderByDescending(ug => ug.UpdatedDate) // updated latest first
                                .AsQueryable();

            var count = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PaginatedList<UserGame>(items, count, page, pageSize); 
        }

        public async  Task<List<UserGame>> GetStatusesByRawgIdsAsync(int userId, List<int> rawgIds)
        {
            return await _context.UserGames
                                .Include(ug => ug.Game)
                                .Where(ug => ug.UserId == userId && rawgIds.Contains(ug.Game.RawgId))
                                .ToListAsync();
        }
    }
}
