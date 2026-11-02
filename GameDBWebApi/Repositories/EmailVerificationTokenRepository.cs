using GameDBWebApi.Data;
using GameDBWebApi.Entites;
using Microsoft.EntityFrameworkCore;

namespace GameDBWebApi.Repositories;

public sealed class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly AppDbContext _context;

    public EmailVerificationTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailVerificationToken token)
    {
        await _context.EmailVerificationTokens.AddAsync(token);
    }

    public async Task InvalidateActiveTokensAsync(int userId)
    {
        var activeTokens = await _context.EmailVerificationTokens
            .Where(token => token.UserId == userId && !token.UsedAtUtc.HasValue)
            .ToListAsync();

        foreach (var activeToken in activeTokens)
        {
            activeToken.UsedAtUtc = DateTime.UtcNow;
        }
    }

    public Task<EmailVerificationToken?> GetByHashAsync(string tokenHash)
    {
        return _context.EmailVerificationTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);
    }
}
