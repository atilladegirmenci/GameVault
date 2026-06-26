using Microsoft.AspNetCore.Mvc;
using GameDBWebApi.DTOs.Auth;

namespace GameDBWebApi.Services
{
    public interface IAuthService
    {
        Task LoginAsync(LoginRequestDto request);

        Task RegisterAsync(RegisterRequestDto request);
    }
}
