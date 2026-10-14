using GameDBWebApi.DTOs.Review;
using GameDBWebApi.Entites;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameDBWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: api/review/game/3498?page=1&pageSize=10
        [AllowAnonymous]
        [HttpGet("game/{rawgId:int}")]
        public async Task<IActionResult> GetGameReviews(
            [FromRoute] int rawgId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (rawgId <= 0)
            {
                return BadRequest(new
                {
                    message = "RAWG ID must be greater than 0."
                });
            }

            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 40)
                pageSize = 40;

            var reviews = await _reviewService.GetGameReviewsAsync(
                rawgId, page, pageSize);

            return Ok(reviews);
        }

        // GET: api/review/15
        [AllowAnonymous]
        [HttpGet("{reviewId:int}")]
        public async Task<IActionResult> GetReviewById(
            [FromRoute] int reviewId)
        {
            if (reviewId <= 0)
            {
                return BadRequest(new
                {
                    message = "Review ID must be greater than 0."
                });
            }

            var review = await _reviewService.GetReviewByIdAsync(reviewId);

            if (review == null)
            {
                return NotFound(new
                {
                    message = $"Review with ID {reviewId} was not found."
                });
            }

            return Ok(review);
        }

        // POST: api/review/game/3498
        [Authorize]
        [HttpPost("game/{rawgId:int}")]
        public async Task<IActionResult> CreateReview(
            [FromRoute] int rawgId,
            [FromBody] CreateReviewRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            var createdReview = await _reviewService.CreateReviewAsync(
                currentUserId,
                rawgId,
                request);

            return CreatedAtAction(
                nameof(GetReviewById),
                new { reviewId = createdReview.Id },
                createdReview);
        }

        // GET: api/review/my-reviews?page=1&pageSize=10
        [Authorize]
        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var currentUserId = GetCurrentUserId();

            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 40)
                pageSize = 40;

            var reviews = await _reviewService.GetUserReviewsAsync(
                currentUserId, page, pageSize);

            return Ok(reviews);
        }

        // DELETE: api/review/15
        [Authorize]
        [HttpDelete("{reviewId:int}")]
        public async Task<IActionResult> DeleteReview(
            [FromRoute] int reviewId)
        {
            var currentUserId = GetCurrentUserId();

            if (reviewId <= 0)
            {
                return BadRequest(new
                {
                    message = "Review ID must be greater than 0."
                });
            }

            var review = await _reviewService.GetReviewByIdAsync(reviewId);

            if (review == null)
            {
                return NotFound(new
                {
                    message = $"Review with ID {reviewId} was not found."
                });
            }

            await _reviewService.DeleteReviewAsync(currentUserId, reviewId);

            return NoContent();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) ||
                !int.TryParse(userIdClaim, out int userId) ||
                userId <= 0)
            {
                throw new UnauthorizedAccessException(
                    "User not found or not authorized.");
            }

            return userId;
        }
    }
}
