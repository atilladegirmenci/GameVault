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
        // Lookup user by stored refresh token fingerprint (SHA256 of token)
        Task<User?> GetByRefreshTokenFingerprintAsync(string fingerprint);
        Task<bool> UserNameExistsAsync(string userName);
    }
}
