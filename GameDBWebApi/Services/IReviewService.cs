using GameDBWebApi.Entites;
using GameDBWebApi.DTOs.Review;
using GameDBWebApi.DTOs;

namespace GameDBWebApi.Services
{
    public interface IReviewService
    {
        Task<PaginatedList<ReviewResponseDto>> GetGameReviewsAsync(
            int RawgId,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
        Task<ReviewResponseDto?> GetReviewByIdAsync(int Id);
        Task<ReviewResponseDto> CreateReviewAsync(
            int userId,
            int rawgId,
            CreateReviewRequestDto request,
            CancellationToken cancellationToken);
        Task<PaginatedList<ReviewResponseDto>> GetUserReviewsAsync(int userId, int page, int pageSize);
        Task DeleteReviewAsync(int userId, int reviewId);
    }
}
