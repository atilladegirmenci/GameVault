
using System.Security.Claims;
using GameDBWebApi.DTOs.List;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameDBWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserListsController : ControllerBase
    {
        private readonly IUserListService _userListService;

        public UserListsController(IUserListService userListService)
        {
            _userListService = userListService;
        }

        [HttpGet("user/{userId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserLists(int userId)
        {
            var result = await _userListService.GetUserListsAsync(userId);
            return Ok(result);
        }

        [HttpGet("{listId:int}/games")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListGames(int listId)
        {
            var result = await _userListService.GetListGamesAsync(listId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateList([FromBody] CreateUserListRequestDto request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _userListService.CreateListAsync(userId, request);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{listId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateList(int listId, [FromBody] UpdateUserListRequestDto request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _userListService.UpdateListAsync(userId, listId, request);

            return Ok(result);
        }

        [HttpDelete("{listId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteList(int listId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            await _userListService.DeleteListAsync(userId, listId);

            return NoContent();
        }

        [HttpPost("{listId:int}/games")]
        [Authorize]
        public async Task<IActionResult> AddGame(int listId, [FromBody] AddGameToListRequestDto request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            await _userListService.AddGameAsync(userId, listId, request.RawgId);

            return NoContent();
        }

        [HttpDelete("{listId:int}/games/{gameId:int}")]
        [Authorize]
        public async Task<IActionResult> RemoveGame(int listId, int gameId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            await _userListService.RemoveGameAsync(userId, listId, gameId);

            return NoContent();
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out userId);
        }
    }
}
