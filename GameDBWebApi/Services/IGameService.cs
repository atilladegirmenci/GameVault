namespace GameDBWebApi.Services;

using GameDBWebApi.DTOs;
using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;

public interface IGameService
{
    Task UpsertGameStatusAsync(int userİd, UpsertGameStatusDto request, CancellationToken cancellationToken);
    Task<GameDetailsResponseDto> GetGameDetailsAsync(int? userId, int catalogId, CancellationToken cancellationToken);
    Task<PaginatedList<GameCardDto>> GetGameFeedAsync(
        int? userId,
        int page,
        int pageSize,
        string ordering,
        CancellationToken cancellationToken);
    Task<PaginatedFeedResponse> SearchGamesAsync(
        int page,
        int pageSize,
        string keyword,
        CancellationToken cancellationToken);
    Task<PaginatedList<GameCardDto>> GetUserGamesAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
