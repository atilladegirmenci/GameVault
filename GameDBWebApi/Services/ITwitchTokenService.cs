namespace GameDBWebApi.Services
{
    public interface ITwitchTokenService
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
    }
}
