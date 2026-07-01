using GameDBWebApi.DTOs.Auth;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameDBWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                LoginResponseDto response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = "Invalid username or password." }); //401
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                await _authService.RegisterAsync(request);
                return Ok(new { message = "Registration successful" });

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

                await _authService.DeleteAsync(id);
                return Ok(new { message = "User deleted successfully" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); //400
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

                await _authService.UpdateAsync(id, request);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); //400
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                return Ok(new { message = "Logout successful" });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message }); //400

            }
        }
    }
}
