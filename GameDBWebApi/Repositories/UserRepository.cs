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
    }
}
