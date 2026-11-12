using Microsoft.AspNetCore.Mvc;
using GameDBWebApi.DTOs.Auth;

namespace GameDBWebApi.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);

        Task LogoutAsync(int userId);
        Task ResendVerificationEmailAsync(string email, CancellationToken cancellationToken);
        Task VerifyEmailAsync(string token);
    }
}
