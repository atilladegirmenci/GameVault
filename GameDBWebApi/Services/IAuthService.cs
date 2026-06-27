using Microsoft.AspNetCore.Mvc;
using GameDBWebApi.DTOs.Auth;

namespace GameDBWebApi.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

        Task RegisterAsync(RegisterRequestDto request);

        Task DeleteAsync(int id);
        Task UpdateAsync(int id, UpdateRequestDto request);
    }
}
