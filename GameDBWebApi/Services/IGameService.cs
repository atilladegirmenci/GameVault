namespace GameDBWebApi.Services;

using GameDBWebApi.DTOs;
using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;

public interface IGameService
{
    Task UpsertGameStatusAsync(int userİd, UpsertGameStatusDto request);
    Task<GameDetailsResponseDto> GetGameDetailsAsync(int? userId, int rawgId);
    Task<PaginatedList<GameCardDto>> GetGameFeedAsync(int? userId, int page, int pageSize, string ordering);
    Task<PaginatedList<GameCardDto>> GetUserGamesAsync(int userId, int page, int pageSize);
}
