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
        private readonly IRawgApiService _rawgApiService;
        private const int MaxGamesPerList = 100; // for now keeping this in this service, but could be moved to a config file or database if needed in the future.

        public UserListService(IUnitOfWork unitOfWork, IRawgApiService rawgApiService)
        {
            _unitOfWork = unitOfWork;
            _rawgApiService = rawgApiService;
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
                RawgId = ulg.Game.RawgId,
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

            var userLists = await _unitOfWork.UserLists.GetByUserIdAsync(userId);

            // TODO: Consider storing GameCount on UserList if list queries become a performance bottleneck.
            //maybe paginated results in the future if a user has a lot of lists, but for now this should be fine.
            return userLists.Select(list => new UserListResponseDto
            {
                Id = list.Id,
                ListTitle = list.ListTitle,
                UserId = list.UserId,
                GameCount = list.ListedGames.Count
            });
        }
        public async Task AddGameAsync(int userId, int listId, int rawgId)
        {
            if (rawgId <= 0)
                throw new BadRequestException(ErrorMessages.Game.RawgGameIdMustBePositive);

            var userList = await GetOwnedListAsync(userId, listId);

            var alreadyExists = await _unitOfWork.UserLists
                .ContainsGameByRawgIdAsync(listId, rawgId);

            if (alreadyExists)
                throw new ConflictException(ErrorMessages.UserList.GameAlreadyInList);

            var gameCount = await _unitOfWork.UserLists
                .GetGameCountByListIdAsync(listId);

            if (gameCount >= MaxGamesPerList)
                throw new BadRequestException(
                    string.Format(ErrorMessages.UserList.ListMaxGamesTemplate, MaxGamesPerList));

            var game = await _unitOfWork.Games.GetByRawgIdAsync(rawgId);

            if (game is null)
            {
                var rawgGame = await _rawgApiService.GetGameDetailsByRawgIdAsync(rawgId);

                if (rawgGame is null)
                    throw new NotFoundException(ErrorMessages.External.GameNotFoundOnRawg);

                game = new Game
                {
                    RawgId = rawgId,
                    Title = rawgGame.Title,
                    ImageUrl = rawgGame.ImageUrl
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
