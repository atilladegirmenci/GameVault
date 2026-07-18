using GameDBWebApi.DTOs.Auth;
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateRequestDto request)
        {
            try
            {
                var currentUserid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (currentUserid != id)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new
                        {
                            message = "You are not authorized to update this user. What are you trying to do?"
                        }); //403
                }

                await _userService.UpdateAsync(id, request);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); //400
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var currentUserid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (currentUserid != id)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new
                        {
                            message = "You are not authorized to delete this user. What are you trying to do?"
                        }); //403
                }

                await _userService.DeleteAsync(id);
                return Ok(new { message = "User deleted successfully" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); //400
            }
        }

    }
}
