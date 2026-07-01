using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;

namespace GameDBWebApi.Services
{
    public interface IRawgApiService
    {
        Task<Game?> GetGameDetailsByRawgIdAsync(int rawgId);
        Task<PaginatedFeedResponse> GetGameFeedAsync(int page, int pageSize, string ordering);
    }
}
