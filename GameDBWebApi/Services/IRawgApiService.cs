using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;

namespace GameDBWebApi.Services
{
    public interface IRawgApiService
    {
        Task<Game?> GetGameByRawgIdAsync(int rawgId);
        Task<GameDetailsResponseDto> GetGameDetailsByRawgIdAsync(int rawgId);
        //Task<PaginatedFeedResponse> GetGameFeedAsync(int page, int pageSize, string ordering);
        Task<PaginatedFeedResponse> GetGamesAsync(int page, int pageSize, string? keyword, string ordering = "-added");


    }
}
