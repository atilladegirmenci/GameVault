using GameDBWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using GameDBWebApi.DTOs.Auth;
using Microsoft.AspNetCore.Identity.Data;

namespace GameDBWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
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
                await _authService.LoginAsync(request);
                return Ok(new { message = "Login successful" });
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
    }
}
