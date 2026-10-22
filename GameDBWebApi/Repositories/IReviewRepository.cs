using GameDBWebApi.Entites;

namespace GameDBWebApi.Repositories
{

    public interface IReviewRepository
    {
        Task AddAsync(Review review);
        Task Delete(Review review);
        Task<Review?> GetByIdAsync(int reviewId);
        Task<List<Review>> GetByGameIdAsync(int gameId, int page, int pageSize);
        Task<List<Review>> GetByUserIdAsync(int userId, int page, int pageSize);
        Task<bool> UserReviewExistsAsync(int gameId, int userId);
        Task<int> GetCountByGameIdAsync(int gameId);
        Task<int> GetCountByUserIdAsync(int userId);
    }
}
