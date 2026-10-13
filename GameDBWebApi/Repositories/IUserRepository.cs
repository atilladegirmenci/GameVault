using GameDBWebApi.Entites;
namespace GameDBWebApi.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUserNameAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetByUserMailAsync(string usermail);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<bool> UserNameExistsAsync(string userName);
    }
}
