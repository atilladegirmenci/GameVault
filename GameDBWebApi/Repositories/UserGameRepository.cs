using GameDBWebApi.Data;
using GameDBWebApi.Entites;
using Microsoft.EntityFrameworkCore;

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
    }
}
