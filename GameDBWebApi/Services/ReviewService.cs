using GameDBWebApi.Constants;
using GameDBWebApi.Entites;
using GameDBWebApi.Exceptions;
using GameDBWebApi.Repositories;
using GameDBWebApi.DTOs.Review;
using GameDBWebApi.DTOs;

namespace GameDBWebApi.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRawgApiService _rawgApiService;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IUnitOfWork unitOfWork,
            IRawgApiService rawgApiService,
            ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _rawgApiService = rawgApiService;
            _logger = logger;
        }

        public async Task<ReviewResponseDto> CreateReviewAsync(
            int userId,
            int rawgId,
            CreateReviewRequestDto request)
        {
            if (rawgId <= 0)
            {
                throw new BadRequestException(
                    ErrorMessages.Review.RawgIdMustBeGreaterThanZero);
            }

            var game = await GetOrCreateGameAsync(rawgId);

            var alreadyReviewed = await _unitOfWork.Reviews
                .UserReviewExistsAsync(game.Id, userId);

            if (alreadyReviewed)
            {
                throw new ConflictException(
                    ErrorMessages.Review.AlreadyReviewed);
            }

            var review = new Review
            {
                GameId = game.Id,
                UserId = userId,
                Rating = request.Rating,
                Comment = request.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Review {ReviewId} created by user {UserId} for game {GameId}.",
                review.Id,
                userId,
                game.Id);

            // Reload the review so its User navigation can be included.
            var createdReview = await _unitOfWork.Reviews
                .GetByIdAsync(review.Id);

            return MapToResponseDto(createdReview ?? review); 
        }

        public async Task DeleteReviewAsync(int userId, int  reviewId)
        {
            var review = await _unitOfWork.Reviews
                 .GetByIdAsync(reviewId);

            if (review == null)
            {
                throw new NotFoundException(
                    string.Format(ErrorMessages.Review.ReviewNotFoundTemplate, reviewId));
            }

            if (review.UserId != userId)
            {
                throw new ForbiddenException(
                    ErrorMessages.Review.DeleteNotAllowed);
            }

            _unitOfWork.Reviews.Delete(review);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                 "Review {ReviewId} deleted by user {UserId}.",
                 reviewId,
                 userId);
        }

        public async Task<PaginatedList<ReviewResponseDto>> GetGameReviewsAsync(
            int RawgId,
            int page,
            int pageSize)
        {
            ValidatePagination(page, pageSize);

            if (RawgId <= 0)
            {
                throw new BadRequestException(
                    ErrorMessages.Review.RawgIdMustBeGreaterThanZero);
            }

            var game = await GetOrCreateGameAsync(RawgId);

            var reviews = await _unitOfWork.Reviews
                .GetByGameIdAsync(game.Id, page, pageSize);
            
            var totalCount = await _unitOfWork.Reviews
               .GetCountByGameIdAsync(game.Id);

            var items = reviews
                .Select(MapToResponseDto)
                .ToList();

            _logger.LogInformation(
                "Retrieved {ReviewCount} reviews for game {GameId}, page {Page}.",
                reviews.Count,
                game.Id,
                page);

            return new PaginatedList<ReviewResponseDto>(
               items,
               totalCount,
               page,
               pageSize);
        }

        public async Task<ReviewResponseDto?> GetReviewByIdAsync(
            int reviewId)
        {
            var review = await _unitOfWork.Reviews
                .GetByIdAsync(reviewId);

            return review == null
                ? null
                : MapToResponseDto(review);
        }

        public async Task<PaginatedList<ReviewResponseDto>>
            GetUserReviewsAsync(int userId, int page, int pageSize)
        {
            ValidatePagination(page, pageSize);

            var reviews = await _unitOfWork.Reviews
                .GetByUserIdAsync(userId, page, pageSize);

            var totalCount = await _unitOfWork.Reviews
                .GetCountByUserIdAsync(userId);

            var items = reviews
                .Select(MapToResponseDto)
                .ToList();

            return new PaginatedList<ReviewResponseDto>(
                items,
                totalCount,
                page,
                pageSize);
        }

        private async Task<Game> GetOrCreateGameAsync(int rawgId)
        {
            var game = await _unitOfWork.Games.GetByRawgIdAsync(rawgId);

            if (game != null)
            {
                return game;
            }

            var rawGameData = await _rawgApiService.GetGameByRawgIdAsync(rawgId);

            if (rawGameData == null)
            {
                throw new NotFoundException(
                    string.Format(ErrorMessages.External.RawgGameNotFoundTemplate, rawgId));
            }

            game = new Game
            {
                RawgId = rawgId,
                Title = rawGameData.Title,
                ImageUrl = rawGameData.ImageUrl,
                ReleaseDate = rawGameData.ReleaseDate,
                Score = rawGameData.Score
            };

            await _unitOfWork.Games.AddAsync(game);
            await _unitOfWork.CompleteAsync();

            return game;
        }

        private static void ValidatePagination(int page, int pageSize)
        {
            if (page < 1)
            {
                throw new BadRequestException(
                    ErrorMessages.Review.PageMustBeGreaterThanOrEqualToOne);
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new BadRequestException(
                    ErrorMessages.Review.PageSizeRange);
            }
        }
      
        private static ReviewResponseDto MapToResponseDto(Review review)
        {
            return new ReviewResponseDto
            {
                Id = review.Id,
                GameId = review.GameId,
                RawgId = review.Game?.RawgId ?? 0,
                UserId = review.UserId,
                UserName = review.User?.UserName ?? string.Empty,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }
    }
}
