using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Repositories;
using System.Security.AccessControl;
using GameDBWebApi.Entites;
using System.Text.Json.Serialization;

namespace GameDBWebApi.Services
{
    public class GameService : IGameService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IRawgApiService _rawgApiService;
        private readonly ILogger<GameService> _logger;

        public GameService(IUnitOfWork unitOfWork, IConfiguration configuration, IRawgApiService rawgApiService, ILogger<GameService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _rawgApiService = rawgApiService;
            _logger = logger;
        }

        public async Task UpsertGameStatusAsync(int userId, UpsertGameStatusDto request)
        {
            var game = await _unitOfWork.Games.GetByRawgIdAsync(request.RawgId);

            if(game == null)
            {
                _logger.LogInformation("Game with RawgId {RawgId} not found in local database. Fetching from RAWG API and saving to db", request.RawgId);

                var rawGameData = await _rawgApiService.GetGameDetailsByRawgIdAsync(request.RawgId);

                if(rawGameData == null)
                {
                    throw new Exception($"Game with RawgId {request.RawgId} not found in RAWG API.");
                }

                game = new Game
                {
                    RawgId = request.RawgId,
                    Title = rawGameData.Title,
                    ImageUrl = rawGameData.ImageUrl,
                    ReleaseDate = rawGameData.ReleaseDate,
                    Score = rawGameData.Score
                };

                await _unitOfWork.Games.AddAsync(game);
                await _unitOfWork.CompleteAsync();
            }

            var userGame = await _unitOfWork.UserGames.GetByUserIDAndGameIdAsync(userId, game.Id);
           _logger.LogInformation("Upserting game for user {userId} and game {gameId}.", userId, game.Id);
           
            if (userGame == null)
            {
                userGame = new UserGame
                {
                    UserId = userId,
                    GameId = game.Id,
                    Status = request.Status,
                    UpdatedDate = DateTime.UtcNow

                };
                
                await _unitOfWork.UserGames.AddAsync(userGame);
            }
            else
            {
                userGame.Status = request.Status;
                userGame.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.UserGames.UpdateAsync(userGame);
            }

            await _unitOfWork.CompleteAsync();
        }

       
    }

  
}
