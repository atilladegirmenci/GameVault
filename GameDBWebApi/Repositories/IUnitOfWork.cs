namespace GameDBWebApi.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IGameRepository Games { get; }
        IUserGameRepository UserGames { get; }
        Task<int> CompleteAsync();
    }
}
