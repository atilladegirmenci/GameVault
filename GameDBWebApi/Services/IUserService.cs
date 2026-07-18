using GameDBWebApi.DTOs.Auth;
using GameDBWebApi.Entites;

namespace GameDBWebApi.Services
{
    public interface IUserService
    {
        Task UpdateAsync(int id, UpdateRequestDto request);

        Task<User> GetUserByIdAsync(int id);

        Task DeleteAsync(int id);

    }
}
