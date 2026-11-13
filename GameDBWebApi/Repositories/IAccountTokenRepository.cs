using GameDBWebApi.Entites;

namespace GameDBWebApi.Repositories;

public interface IAccountTokenRepository
{
    Task AddAsync(AccountToken token, CancellationToken cancellationToken = default);
    Task InvalidateActiveTokensAsync(
        int userId,
        AccountTokenPurpose purpose,
        CancellationToken cancellationToken = default);
    Task<AccountToken?> GetByHashAsync(
        string tokenHash,
        AccountTokenPurpose purpose,
        CancellationToken cancellationToken = default);
}
