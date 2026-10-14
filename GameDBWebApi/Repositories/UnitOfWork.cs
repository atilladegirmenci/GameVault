using GameDBWebApi.Data;

namespace GameDBWebApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IUserRepository Users {  get; private set; }
        public IGameRepository Games { get; private set; }
        public IUserGameRepository UserGames { get; private set; }
        public IReviewRepository Reviews { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Games = new GameRepository(_context);
            UserGames = new UserGameRepository(_context);
            Reviews = new ReviewRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
