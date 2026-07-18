using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;
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
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if(currentUserId == 0)
                {
                    return Unauthorized(new { message = "User not found or not authorized" });
                }

                await _gameService.UpsertGameStatusAsync(currentUserId, request);

                return Ok(new { message = $"Game status updated successfully to '{request.Status}'" });
            }
            catch(Exception ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("my-games")]
        public async Task<IActionResult> GetUserGames([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
               
                if(currentUserId == 0)
                {
                    return Unauthorized(new { message = "User not found or not authorized" });
                }

                if (pageSize > 40) pageSize = 40;
                if (page < 1) page = 1;
                var userGames = await _gameService.GetUserGamesAsync(currentUserId, page, pageSize);
                return Ok(userGames);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("feed")]
        public async Task<IActionResult> GetGameFeed(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string ordering = "added")
        {
            int? userId = null;
            try
            {
                if (User?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
                    {
                        userId = parsedId;
                    }
                }

                //rpevent abusive requests and invalid page numbers
                if (pageSize > 40) pageSize = 40;  
                if(page <1) page = 1; 

                var feed = await _gameService.GetGameFeedAsync(userId, page, pageSize, ordering);

                return Ok(feed);
            }
            catch (Exception ex)
            {
                
                return BadRequest(new { message = ex.Message });
            }

        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchGameByTitle(
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if(string.IsNullOrEmpty(keyword))
                {
                    return BadRequest(new { message = "Keyword cannot be null or empty" });
                }

                if (pageSize > 40) pageSize = 40;
                if (page < 1) page = 1;

                var searchResults = await _rawgApiService.GetGamesAsync(page, pageSize, keyword, "-added");

                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("{rawgId}")]
        public async Task<IActionResult> GetGameDetails([FromRoute] int rawgId)
        {
            int? userId = null;
            try
            {
                if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
                    {
                        userId = parsedId;
                    }
                }

                var game = await _gameService.GetGameDetailsAsync(userId, rawgId);
               
                return Ok(game);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
