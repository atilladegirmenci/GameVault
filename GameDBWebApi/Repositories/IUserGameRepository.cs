using GameDBWebApi.DTOs;
using GameDBWebApi.Entites;

namespace GameDBWebApi.Repositories
{
    public interface IUserGameRepository
    {
        Task<UserGame?> GetByUserIDAndGameIdAsync(int userId, int gameId);
        Task AddAsync(UserGame userGame);
        Task UpdateAsync(UserGame userGame);
        Task<PaginatedList<UserGame>> GetByUserIdAsync(int userId, int page, int pageSize);
        Task<List<UserGame>> GetStatusesByRawgIdsAsync(int userId, List<int> rawgIds);
    }
}
