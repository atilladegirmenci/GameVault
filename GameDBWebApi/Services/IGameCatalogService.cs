using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;

namespace GameDBWebApi.Services
{
    public interface IGameCatalogService
    {
        Task<Game?> GetGameByCatalogIdAsync(int catalogId, CancellationToken cancellationToken);
        Task<GameDetailsResponseDto> GetGameDetailsByCatalogIdAsync(int catalogId, CancellationToken cancellationToken);
        Task<PaginatedFeedResponse> GetGamesByCatalogAsync(
            int page,
            int pageSize,
            string? keyword,
            string ordering,
            CancellationToken cancellationToken);


    }
}
