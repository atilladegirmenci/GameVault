
using GameDBWebApi.Data;
using GameDBWebApi.Entites;
using Microsoft.EntityFrameworkCore;

namespace GameDBWebApi.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        public Task Delete(Review review)
        {
            _context.Reviews.Remove(review);
            return Task.CompletedTask;
        }

        public async Task<List<Review>> GetByGameIdAsync(
            int gameId,
            int page,
            int pageSize)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Game)
                .Include(r => r.User)
                .Where(r => r.GameId == gameId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Review?> GetByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        public async Task<List<Review>> GetByUserIdAsync(
            int userId,
            int page,
            int pageSize)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Game)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UserReviewExistsAsync(int gameId, int userId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.GameId == gameId && r.UserId == userId);
        }
        public async Task<int> GetCountByGameIdAsync(int gameId)
        {
            return await _context.Reviews
                .CountAsync(r => r.GameId == gameId);
        }

        public async Task<int> GetCountByUserIdAsync(int userId)
        {
            return await _context.Reviews
                .CountAsync(r => r.UserId == userId);
        }
    }
}
