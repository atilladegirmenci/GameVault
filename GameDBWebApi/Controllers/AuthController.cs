using GameDBWebApi.DTOs.Auth;
using GameDBWebApi.Constants;
using GameDBWebApi.Exceptions;
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDto request,
            CancellationToken cancellationToken)
        {
            await _authService.RegisterAsync(request, cancellationToken);
            return Ok(new { message = "Registration successful" });
        }

        [AllowAnonymous]
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification(
            [FromBody] ResendVerificationRequestDto request,
            CancellationToken cancellationToken)
        {
            await _authService.ResendVerificationEmailAsync(request.Email, cancellationToken);
            return Ok(new { message = "If the account exists and is unverified, a verification email has been sent." });
        }

        [AllowAnonymous]
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(
            [FromQuery] string token,
            CancellationToken cancellationToken)
        {
            await _authService.VerifyEmailAsync(token, cancellationToken);
            return Ok(new { message = "Email verified successfully." });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequestDto request,
            CancellationToken cancellationToken)
        {
            await _authService.RequestPasswordResetAsync(request, cancellationToken);
            return Ok(new { message = ErrorMessages.Auth.PasswordResetRequested });
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequestDto request,
            CancellationToken cancellationToken)
        {
            await _authService.ResetPasswordAsync(request, cancellationToken);
            return Ok(new { message = ErrorMessages.Auth.PasswordChangedSuccessfully });
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {

            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token claims." });
            }

            await _authService.LogoutAsync(userId);
            return Ok(new { message = "Logout successful" });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequestDto request,
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = ErrorMessages.Auth.InvalidTokenClaims });
            }

            await _authService.ChangePasswordAsync(userId, request, cancellationToken);
            return Ok(new { message = ErrorMessages.Auth.PasswordChangedSuccessfully });
        }
    }
}
