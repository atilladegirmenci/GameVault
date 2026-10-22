using GameDBWebApi.DTOs.User;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GameDBWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var currentUserId = GetCurrentUserId();

            var profile = await _userService.GetUserByIdAsync(currentUserId);
            return Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            await _userService.UpdateAsync(currentUserId, request);
            return Ok(new { message = "User updated successfully" });
        }

        [Authorize]
        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteAccount()
        {
            var currentUserId = GetCurrentUserId();

            await _userService.DeleteAsync(currentUserId);
            return Ok(new { message = "User deleted successfully" });
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

    }
}
