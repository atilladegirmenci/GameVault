
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
        Task<IEnumerable<UserListGameResponseDto>> GetListGamesAsync(int listId); //TODO: Consider returning DTOs instead of entities for better abstraction

        Task AddGameAsync(
            int userId,
            int listId,
            int catalogId,
            CancellationToken cancellationToken);

        Task RemoveGameAsync(
            int userId,
            int listId,
            int gameId);

        Task ReorderGamesAsync(int userId, int listId, IReadOnlyList<int> orderedGameIds);
    }
}
