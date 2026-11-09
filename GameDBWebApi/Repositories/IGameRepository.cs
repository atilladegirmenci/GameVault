using GameDBWebApi.Entites;

namespace GameDBWebApi.Repositories
{
   
    public interface IGameRepository
    {
        Task<Game?> GetByCatalogIdAsync(int catalogId);
        Task AddAsync(Game game);

        Task<Game?> GetByIdAsync(int gameId);
    }
}
