using GameDBWebApi.Constants;
using GameDBWebApi.DTOs.List;
using GameDBWebApi.Entites;
using GameDBWebApi.Exceptions;
using GameDBWebApi.Repositories;

namespace GameDBWebApi.Services
{
    public class UserListService : IUserListService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGameCatalogService _catalogApiService;
        private const int MaxGamesPerList = 100; // for now keeping this in this service, but could be moved to a config file or database if needed in the future.

        public UserListService(IUnitOfWork unitOfWork, IGameCatalogService catalogApiService)
        {
            _unitOfWork = unitOfWork;
            _catalogApiService = catalogApiService;
        }


        public async Task<UserListResponseDto> CreateListAsync(int userId, CreateUserListRequestDto request)
        {
            var userList = new UserList
            {
                UserId = userId,
                ListTitle = request.ListTitle.Trim(),
            };

            await _unitOfWork.UserLists.AddAsync(userList);
            await _unitOfWork.CompleteAsync();

            return new UserListResponseDto
            {
                Id = userList.Id,
                ListTitle = userList.ListTitle,
                UserId = userList.UserId,
                GameCount = 0
            };
        }

        public async Task DeleteListAsync(int userId, int listId)
        {
            var userList = await GetOwnedListAsync(userId, listId);

            _unitOfWork.UserLists.Delete(userList);
            await _unitOfWork.CompleteAsync();
        }
        public async Task<UserListResponseDto> UpdateListAsync(int userId, int listId, UpdateUserListRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.ListTitle))
            {
                throw new BadRequestException(ErrorMessages.UserList.ListTitleCannotBeNullOrEmpty);
            }

            var userList = await GetOwnedListAsync(userId, listId);

            userList.ListTitle = request.ListTitle.Trim();

            _unitOfWork.UserLists.Update(userList);
            await _unitOfWork.CompleteAsync();

            return new UserListResponseDto
            {
                Id = userList.Id,
                ListTitle = userList.ListTitle,
                UserId = userList.UserId,
                GameCount = userList.ListedGames?.Count ?? 0
            };
        }

        public async Task<IEnumerable<UserListGameResponseDto>> GetListGamesAsync(int listId)
        {
            var userList = await _unitOfWork.UserLists.GetByIdAsync(listId);

            if (userList is null)
                throw new NotFoundException(ErrorMessages.UserList.UserListNotFound);

            var listGames = await _unitOfWork.UserLists.GetGamesByListIdAsync(listId);

            return listGames.Select(ulg => new UserListGameResponseDto
            {
                GameId = ulg.GameId,
                CatalogId = ulg.Game.CatalogId,
                Title = ulg.Game.Title,
                ImageUrl = ulg.Game.ImageUrl,
                AddedDate = ulg.AddedDate
            });
        }

        public async Task<IEnumerable<UserListResponseDto>> GetUserListsAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user is null)
                throw new NotFoundException(ErrorMessages.User.UserNotFound);

            var userLists = await _unitOfWork.UserLists.GetSummariesByUserIdAsync(userId, previewCount: 5);

            return userLists;
        }
        public async Task AddGameAsync(
            int userId,
            int listId,
            int catalogId,
            CancellationToken cancellationToken)
        {
            if (catalogId <= 0)
                throw new BadRequestException(ErrorMessages.Game.CatalogGameIdMustBePositive);

            var userList = await GetOwnedListAsync(userId, listId);

            var alreadyExists = await _unitOfWork.UserLists
                .ContainsGameByCatalogIdAsync(listId, catalogId);

            if (alreadyExists)
                throw new ConflictException(ErrorMessages.UserList.GameAlreadyInList);

            var gameCount = await _unitOfWork.UserLists
                .GetGameCountByListIdAsync(listId);

            if (gameCount >= MaxGamesPerList)
                throw new BadRequestException(
                    string.Format(ErrorMessages.UserList.ListMaxGamesTemplate, MaxGamesPerList));

            var game = await _unitOfWork.Games.GetByCatalogIdAsync(catalogId);

            if (game is null)
            {
                var catalogGame = await _catalogApiService.GetGameDetailsByCatalogIdAsync(catalogId, cancellationToken);

                if (catalogGame is null)
                    throw new NotFoundException(ErrorMessages.External.GameNotFoundOnCatalog);

                game = new Game
                {
                    CatalogId = catalogId,
                    Title = catalogGame.Title,
                    ImageUrl = catalogGame.ImageUrl
                };

                await _unitOfWork.Games.AddAsync(game);
                await _unitOfWork.CompleteAsync();
            }

            await _unitOfWork.UserLists.AddGameAsync(listId, game.Id);

            _unitOfWork.UserLists.Update(userList);

            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveGameAsync(int userId, int listId, int gameId)
        {
            var userList = await GetOwnedListAsync(userId, listId);

            var gameExists = await _unitOfWork.UserLists
                .ContainsGameAsync(listId, gameId);

            if (!gameExists)
                throw new NotFoundException(ErrorMessages.UserList.GameNotFoundInList);

            await _unitOfWork.UserLists.RemoveGameAsync(listId, gameId);

            _unitOfWork.UserLists.Update(userList);

            await _unitOfWork.CompleteAsync();
        }

        public async Task ReorderGamesAsync(int userId, int listId, IReadOnlyList<int> orderedGameIds)
        {
            var userList = await GetOwnedListAsync(userId, listId);
            var currentEntries = await _unitOfWork.UserLists.GetGamesByListIdAsync(listId);
            var currentGameIds = currentEntries.Select(entry => entry.GameId).ToHashSet();

            if (orderedGameIds is null ||
                orderedGameIds.Count != currentGameIds.Count ||
                orderedGameIds.Distinct().Count() != orderedGameIds.Count ||
                !orderedGameIds.ToHashSet().SetEquals(currentGameIds))
            {
                throw new BadRequestException("The ordered game IDs must contain every game in the list exactly once.");
            }

            await _unitOfWork.UserLists.ReorderGamesAsync(listId, orderedGameIds);
            _unitOfWork.UserLists.Update(userList);
            await _unitOfWork.CompleteAsync();
        }

        private async Task<UserList> GetOwnedListAsync(int userId, int listId)
        {
            var userList = await _unitOfWork.UserLists.GetByIdAsync(listId);

            if (userList is null)
                throw new NotFoundException(ErrorMessages.UserList.UserListNotFound);

            if (userList.UserId != userId)
                throw new ForbiddenException(ErrorMessages.UserList.NoPermissionForList);

            return userList;
        }
    }
}
