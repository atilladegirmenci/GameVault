
using GameDBWebApi.DTOs.List;

namespace GameDBWebApi.Services
{
    public interface IUserListService
    {
        // User list operations
        Task<UserListResponseDto> CreateListAsync(int userId, CreateUserListRequestDto request);

        Task<IEnumerable<UserListResponseDto>> GetUserListsAsync(int userId);

        Task<UserListResponseDto> UpdateListAsync(
            int userId,
            int listId,
            UpdateUserListRequestDto request);

        Task DeleteListAsync(
            int userId,
            int listId);

        // Games within a user list
        Task<IEnumerable<UserListGameResponseDto>> GetListGamesAsync(int listId);

        Task AddGameAsync(
            int userId,
            int listId,
            int rawgId);

        Task RemoveGameAsync(
            int userId,
            int listId,
            int gameId);
    }
}
