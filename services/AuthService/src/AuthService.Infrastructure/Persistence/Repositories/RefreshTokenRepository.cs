using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens.SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
    }

    public async Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(Guid refreshTokenId, CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens.SingleOrDefaultAsync(rt => rt.Id == refreshTokenId, cancellationToken);
        if (token is null)
            return;

        token.Revoke();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        if (activeTokens.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
