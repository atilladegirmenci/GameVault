using GameDBWebApi.DTOs.User;
using GameDBWebApi.Entites;

namespace GameDBWebApi.Services
{
    public interface IUserService
    {
        Task UpdateAsync(int id, UpdateRequestDto request);
        Task<UserProfileDto> GetUserByIdAsync(int id);

        Task DeleteAsync(int id);

    }
}
