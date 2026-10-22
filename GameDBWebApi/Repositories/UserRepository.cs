using GameDBWebApi.Data;
using GameDBWebApi.Entites;
using Microsoft.EntityFrameworkCore;

namespace GameDBWebApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
        }

        public Task<User?> GetByIdAsync(int id)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUserNameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            return null != await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == email);
        }
        public async Task<User?> GetByUserMailAsync(string usermail)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == usermail);
        }
        // Fingerprint-only lookup: caller should compute SHA256 fingerprint and pass it here
        // This avoids storing any form of the token in the database.

        public async Task<User?> GetByRefreshTokenFingerprintAsync(string fingerprint)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    u => u.RefreshTokenFingerprint == fingerprint);
        }
        public async Task<bool> UserNameExistsAsync(string userName)
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName);
        }
    }
}
