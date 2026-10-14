using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameDBWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IRawgApiService _rawgApiService;

        public GameController(IGameService gameService, IRawgApiService rawgApiService)
        {
            _gameService = gameService;
            _rawgApiService = rawgApiService;
        }

        [Authorize]
        [HttpPost("status")]
        public async Task<IActionResult> UpdateGameStatus([FromBody] UpsertGameStatusDto request)
        {
            var currentUserId = GetCurrentUserId();

            await _gameService.UpsertGameStatusAsync(currentUserId, request);

            return Ok(new { message = $"Game status updated successfully to '{request.Status}'" });
        }

        [Authorize]
        [HttpGet("my-games")]
        public async Task<IActionResult> GetUserGames([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var currentUserId = GetCurrentUserId();

            if (pageSize > 40) pageSize = 40;
            if (page < 1) page = 1;

            var userGames = await _gameService.GetUserGamesAsync(currentUserId, page, pageSize);
            return Ok(userGames);
        }

        [AllowAnonymous]
        [HttpGet("feed")]
        public async Task<IActionResult> GetGameFeed(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string ordering = "added") //TODO: Make ordering an enum or a set of predefined values to avoid invalid inputs
        {
            int? userId = GetOptionalUserId();

            if (pageSize > 40) pageSize = 40;
            if (page < 1) page = 1;

            var feed = await _gameService.GetGameFeedAsync(userId, page, pageSize, ordering);

            return Ok(feed);
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchGameByTitle(
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Keyword cannot be null or empty" });
            }

            if (pageSize > 40) pageSize = 40;
            if (page < 1) page = 1;

            var searchResults = await _rawgApiService.GetGamesAsync(page, pageSize, keyword, "-added");

            return Ok(searchResults);
        }

        [AllowAnonymous]
        [HttpGet("{rawgId}")]
        public async Task<IActionResult> GetGameDetails([FromRoute] int rawgId)
        {
            int? userId = GetOptionalUserId();

            var game = await _gameService.GetGameDetailsAsync(userId, rawgId);

            return Ok(game);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId <= 0)
            {
                throw new UnauthorizedAccessException("User not found or not authorized.");
            }

            return userId;
        }

       
        private int? GetOptionalUserId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
                {
                    return parsedId;
                }
            }
            return null;
        }

    }
}