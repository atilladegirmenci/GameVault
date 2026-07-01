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

        [AllowAnonymous]
        [HttpGet("feed")]
        public async Task<IActionResult> GetGameFeed(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string ordering = "-released")
        {
            try
            {
                //rpevent abusive requests and invalid page numbers
                if (pageSize > 40) pageSize = 40;  
                if(page <1) page = 1; 

                var feed = await _rawgApiService.GetGameFeedAsync(page, pageSize, ordering);

                if(feed == null)
                {
                    return StatusCode(StatusCodes.Status502BadGateway, new {message = "Failed to communicate with the external game provider"});
                }

                return Ok(feed);
            }
            catch (Exception ex)
            {
                
                return BadRequest(new { message = ex.Message });
            }

        }

    }
}
