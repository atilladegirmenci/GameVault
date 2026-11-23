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

            await _unitOfWork.UserLists.SyncGameStatusListAsync(userId, game.Id, request.Status);
            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveGameFromLibraryAsync(
            int userId,
            int catalogId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var game = await _unitOfWork.Games.GetByCatalogIdAsync(catalogId);
            if (game == null)
            {
                return;
            }

            var userGame = await _unitOfWork.UserGames.GetByUserIDAndGameIdAsync(userId, game.Id);
            if (userGame == null)
            {
                return;
            }

            await _unitOfWork.UserGames.DeleteAsync(userGame);
            await _unitOfWork.UserLists.RemoveGameFromStatusListsAsync(userId, game.Id);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Removed game {CatalogId} from user {UserId}'s library.",
                catalogId,
                userId);
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

            if (userGames == null || userGames.Items.Count == 0)
            {
                _logger.LogInformation("No games found for user {userId}.", userId);
                return new PaginatedList<GameCardDto>(
                    new List<GameCardDto>(),
                    userGames?.TotalCount ?? 0,
                    page,
                    pageSize);
            }

            var gameCardDtos = userGames.Items.Select(ug => new GameCardDto
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
            GameFeedOrdering ordering,
            SortDirection direction,
            CancellationToken cancellationToken)
        {
            var feed = await _catalogApiService.GetGamesByCatalogAsync(
                page,
                pageSize,
                null,
                ordering,
                direction,
                cancellationToken);

            var items = feed.Games.Select(g => new GameCardDto
            {
                CatalogId = g.CatalogId,
                Title = g.Title,
                ImageUrl = g.ImageUrl,
                Score = g.Score
            }).ToList();

            if (userId.HasValue && items.Count > 0)
            {
                var catalogIds = items.Select(x => x.CatalogId).ToList();

                var userGameStatuses = await _unitOfWork.UserGames.GetStatusesByCatalogIdsAsync(userId.Value, catalogIds);

                //using dictionary o(1) lookup 
                var statusDict = userGameStatuses.ToDictionary(s => s.Game.CatalogId, s => s);

                foreach (var item in items)
                {
                    if (statusDict.TryGetValue(item.CatalogId, out var status))
                    {
                        item.GameId = status.GameId;
                        item.Status = status.Status;
                        item.UpdatedDate = status.UpdatedDate;
                    }
                }
            }

            return new PaginatedList<GameCardDto>(items, feed.TotalCount, page, pageSize);
        }
        public async Task<PaginatedList<GameCardDto>> SearchGamesAsync(
            int? userId,
            int page,
            int pageSize,
            string keyword,
            CancellationToken cancellationToken)
        {
            var feed = await _catalogApiService.GetGamesByCatalogAsync(
                page,
                pageSize,
                keyword,
                GameFeedOrdering.Newest,
                SortDirection.Descending,
                cancellationToken);

            if (userId.HasValue && feed.Games.Count > 0)
            {
                var catalogIds = feed.Games.Select(game => game.CatalogId).ToList();
                var userGameStatuses = await _unitOfWork.UserGames.GetStatusesByCatalogIdsAsync(
                    userId.Value,
                    catalogIds);
                var statusByCatalogId = userGameStatuses.ToDictionary(
                    status => status.Game.CatalogId,
                    status => status);

                foreach (var game in feed.Games)
                {
                    if (statusByCatalogId.TryGetValue(game.CatalogId, out var status))
                    {
                        game.GameId = status.GameId;
                        game.Status = status.Status;
                        game.UpdatedDate = status.UpdatedDate;
                    }
                }
            }

            return new PaginatedList<GameCardDto>(
                feed.Games,
                feed.TotalCount,
                page,
                pageSize);
        }


    }
}
