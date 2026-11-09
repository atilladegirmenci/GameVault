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

        public async Task<Game?> GetByCatalogIdAsync(int catalogId)
        {
            return await _context.Games.FirstOrDefaultAsync(g => g.CatalogId == catalogId);
        }

        public async Task<Game?> GetByIdAsync(int gameId)
        {
            return await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);
        }
    }
}
