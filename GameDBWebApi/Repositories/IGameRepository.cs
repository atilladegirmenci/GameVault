using GameDBWebApi.Entites;

namespace GameDBWebApi.Repositories
{
   
    public interface IGameRepository
    {
        Task<Game?> GetByRawgIdAsync(int rawgId);
        Task AddAsync(Game game);

        Task<Game?> GetByIdAsync(int gameId);
    }
}
