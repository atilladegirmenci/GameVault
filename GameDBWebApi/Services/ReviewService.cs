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
                    "RAWG game ID must be greater than zero.");
            }

            ValidateReview(request);

            var game = await _unitOfWork.Games
                .GetByRawgIdAsync(rawgId);

            if (game == null)
            {
                var rawGameData = await _rawgApiService
                    .GetGameByRawgIdAsync(rawgId);

                if (rawGameData == null)
                {
                    throw new NotFoundException(
                        $"Game with RAWG ID {rawgId} was not found.");
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
            }

            var alreadyReviewed = await _unitOfWork.Reviews
                .UserReviewExistsAsync(game.Id, userId);

            if (alreadyReviewed)
            {
                throw new ConflictException(
                    "You have already reviewed this game.");
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

            return MapToResponseDto(createdReview ?? review); // TODO: userName ve rawgId alanları boş dönüyor; bunları service katmanında dinamik olarak doldur.
        }

        public async Task DeleteReviewAsync(int userId, int  reviewId)
        {
            var review = await _unitOfWork.Reviews
                 .GetByIdAsync(reviewId);

            if (review == null)
            {
                throw new NotFoundException(
                    $"Review with ID {reviewId} was not found.");
            }

            if (review.UserId != userId)
            {
                throw new ForbiddenException(
                    "You are not allowed to delete this review.");
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
                    "RAWG game ID must be greater than zero.");
            }

            var game = await _unitOfWork.Games
                .GetByRawgIdAsync(RawgId);

            if (game == null)
            {
                _logger.LogInformation(
                    "Game with RAWG ID {RawgId} not found locally. Fetching from RAWG API.",
                    RawgId);

                var rawGameData = await _rawgApiService
                    .GetGameByRawgIdAsync(RawgId);

                if (rawGameData == null)
                {
                    throw new NotFoundException(
                        $"Game with RAWG ID {RawgId} was not found.");
                }

                game = new Game
                {
                    RawgId = RawgId,
                    Title = rawGameData.Title,
                    ImageUrl = rawGameData.ImageUrl,
                    ReleaseDate = rawGameData.ReleaseDate,
                    Score = rawGameData.Score
                };

                await _unitOfWork.Games.AddAsync(game);
                await _unitOfWork.CompleteAsync();
            }

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

        private static void ValidatePagination(int page, int pageSize)
        {
            if (page < 1)
            {
                throw new BadRequestException(
                    "Page must be greater than or equal to 1.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new BadRequestException(
                    "Page size must be between 1 and 100.");
            }
        }
        private static void ValidateReview(CreateReviewRequestDto review)
        {
            if (review == null)
            {
                throw new BadRequestException(
                    "Review cannot be null.");
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                throw new BadRequestException(
                    "Rating must be between 1 and 5.");
            }

            if (string.IsNullOrWhiteSpace(review.Comment))
            {
                throw new BadRequestException(
                    "Review comment cannot be empty.");
            }

            if (review.Comment.Length > 2000)
            {
                throw new BadRequestException(
                    "Review comment cannot exceed 2000 characters.");
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
