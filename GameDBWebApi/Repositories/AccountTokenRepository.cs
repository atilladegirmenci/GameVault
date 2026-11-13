using GameDBWebApi.Data;
using GameDBWebApi.Entites;
using Microsoft.EntityFrameworkCore;

namespace GameDBWebApi.Repositories;

public sealed class AccountTokenRepository : IAccountTokenRepository
{
    private readonly AppDbContext _context;

    public AccountTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AccountToken token, CancellationToken cancellationToken = default)
    {
        await _context.AccountTokens.AddAsync(token, cancellationToken);
    }

    public async Task InvalidateActiveTokensAsync(
        int userId,
        AccountTokenPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await _context.AccountTokens
            .Where(token =>
                token.UserId == userId &&
                token.Purpose == purpose &&
                !token.UsedAtUtc.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.UsedAtUtc = DateTime.UtcNow;
        }
    }

    public Task<AccountToken?> GetByHashAsync(
        string tokenHash,
        AccountTokenPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        return _context.AccountTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash && token.Purpose == purpose,
                cancellationToken);
    }
}
