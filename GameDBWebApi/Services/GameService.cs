using GameDBWebApi.DTOs;
using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;
using GameDBWebApi.Exceptions;
using GameDBWebApi.Repositories;

namespace GameDBWebApi.Services
{
    public class GameService : IGameService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGameCatalogService _catalogApiService;
        private readonly ILogger<GameService> _logger;

        public GameService(IUnitOfWork unitOfWork, IGameCatalogService catalogApiService, ILogger<GameService> logger)
        {
            _unitOfWork = unitOfWork;
            _catalogApiService = catalogApiService;
            _logger = logger;
        }

        public async Task UpsertGameStatusAsync(
            int userId,
            UpsertGameStatusDto request,
            CancellationToken cancellationToken)
        {
            var game = await _unitOfWork.Games.GetByCatalogIdAsync(request.CatalogId);

            if (game == null)
            {
                _logger.LogInformation("Game with CatalogId {CatalogId} not found in local database. Fetching from IGDB and saving to db", request.CatalogId);

                var rawGameData = await _catalogApiService.GetGameByCatalogIdAsync(request.CatalogId, cancellationToken);

                game = new Game
                {
                    CatalogId = request.CatalogId,
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

        public async Task<GameDetailsResponseDto> GetGameDetailsAsync(
            int? userId,
            int catalogId,
            CancellationToken cancellationToken)
        {
            var gameDetails = await _catalogApiService.GetGameDetailsByCatalogIdAsync(catalogId, cancellationToken);

            if (userId == null)
            {
                return gameDetails;
            }

            var gameInDb = await _unitOfWork.Games.GetByCatalogIdAsync(catalogId);

            if (gameInDb != null)
            {
                var userGame = await _unitOfWork.UserGames.GetByUserIDAndGameIdAsync(userId.Value, gameInDb.Id);

                if (userGame != null)
                {
                    _logger.LogInformation("Game with CatalogId {CatalogId} is in user {userId}'s library with status {status}.", catalogId, userId, userGame.Status);
                    gameDetails.IsInMyLibrary = true;
                    gameDetails.MyStatus = userGame.Status;
                }
            }

            return gameDetails;
        }
        public async Task<PaginatedList<GameCardDto>> GetUserGamesAsync(
            int userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var userGames = await _unitOfWork.UserGames.GetByUserIdAsync(userId, page, pageSize);

            if (userGames == null || !userGames.Any())
            {
                _logger.LogInformation("No games found for user {userId}.", userId);
                return new PaginatedList<GameCardDto>(new List<GameCardDto>(), 0, page, pageSize);
            }

            var gameCardDtos = userGames.Select(ug => new GameCardDto
            {
                CatalogId = ug.Game.CatalogId,
                Title = ug.Game.Title,
                ImageUrl = ug.Game.ImageUrl,
                Score = ug.Game.Score,
                GameId = ug.Game.Id,
                Status = ug.Status,
                UpdatedDate = ug.UpdatedDate
            }).ToList();

            return new PaginatedList<GameCardDto>(gameCardDtos, userGames.TotalCount, page, pageSize);
        }

        public async Task<PaginatedList<GameCardDto>> GetGameFeedAsync(
            int? userId,
            int page,
            int pageSize,
            string ordering,
            CancellationToken cancellationToken)
        {
            var feed = await _catalogApiService.GetGamesByCatalogAsync(
                page,
                pageSize,
                null,
                ordering,
                cancellationToken);

            var dtoList = new PaginatedList<GameCardDto>(feed.Games.Select(g => new GameCardDto
            {
                CatalogId = g.CatalogId,
                Title = g.Title,
                ImageUrl = g.ImageUrl,
                Score = g.Score
            }).ToList(), feed.TotalCount, page, pageSize);

            if (userId.HasValue && dtoList.Any())
            {
                var catalogIds = dtoList.Select(x => x.CatalogId).ToList();

                var userGameStatuses = await _unitOfWork.UserGames.GetStatusesByCatalogIdsAsync(userId.Value, catalogIds);

                //using dictionary o(1) lookup 
                var statusDict = userGameStatuses.ToDictionary(s => s.Game.CatalogId, s => s);

                foreach (var item in dtoList)
                {
                    if (statusDict.TryGetValue(item.CatalogId, out var status))
                    {
                        item.GameId = status.GameId;
                        item.Status = status.Status;
                        item.UpdatedDate = status.UpdatedDate;
                    }
                }
            }

            return new PaginatedList<GameCardDto>(dtoList, feed.TotalCount, page, pageSize);
        }

        public async Task<PaginatedFeedResponse> SearchGamesAsync(
            int page,
            int pageSize,
            string keyword,
            CancellationToken cancellationToken)
        {
            return await _catalogApiService.GetGamesByCatalogAsync(
                page,
                pageSize,
                keyword,
                "-added",
                cancellationToken);
        }


    }
}
