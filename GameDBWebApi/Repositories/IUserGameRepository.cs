using GameDBWebApi.Entites;

namespace GameDBWebApi.Repositories
{
    public interface IUserGameRepository
    {
        Task<UserGame?> GetByUserIDAndGameIdAsync(int userId, int gameId);
        Task AddAsync(UserGame userGame);
        Task UpdateAsync(UserGame userGame);
    }
}
