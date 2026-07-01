namespace GameDBWebApi.Services;

using GameDBWebApi.DTOs.Game;

public interface IGameService
{
    Task UpsertGameStatusAsync(int userİd, UpsertGameStatusDto request);
}
