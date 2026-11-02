using GameDBWebApi.Entites;

namespace GameDBWebApi.Repositories;

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token);
    Task InvalidateActiveTokensAsync(int userId);
    Task<EmailVerificationToken?> GetByHashAsync(string tokenHash);
}
