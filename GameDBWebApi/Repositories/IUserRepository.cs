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
    }
}
