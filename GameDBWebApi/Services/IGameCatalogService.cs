using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;

namespace GameDBWebApi.Services
{
    public interface IGameCatalogService
    {
        Task<Game?> GetGameByRawgIdAsync(int rawgId, CancellationToken cancellationToken);
        Task<GameDetailsResponseDto> GetGameDetailsByRawgIdAsync(int rawgId, CancellationToken cancellationToken);
        //Task<PaginatedFeedResponse> GetGameFeedAsync(int page, int pageSize, string ordering);
        Task<PaginatedFeedResponse> GetGamesAsync(
            int page,
            int pageSize,
            string? keyword,
            string ordering,
            CancellationToken cancellationToken);


    }
}
