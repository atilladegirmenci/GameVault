using GameDBWebApi.Data;
using Microsoft.EntityFrameworkCore;
using GameDBWebApi.Entites;

namespace GameDBWebApi.Repositories
{
    public class GameRepository : IGameRepository
    {   
        private readonly AppDbContext _context;
        
        public GameRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Game game)
        {
            await _context.Games.AddAsync(game);
        }

        public async Task<Game?> GetByRawgIdAsync(int rawgId)
        {
            return await _context.Games.FirstOrDefaultAsync(g => g.RawgId == rawgId);
        }
    }
}
