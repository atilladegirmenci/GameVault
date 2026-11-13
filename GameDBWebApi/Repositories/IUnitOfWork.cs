namespace GameDBWebApi.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IGameRepository Games { get; }
        IUserGameRepository UserGames { get; }
        IReviewRepository Reviews { get; }
        IUserListRepository UserLists { get; }
        IAccountTokenRepository AccountTokens { get; }
        Task<int> CompleteAsync();
    }
}
