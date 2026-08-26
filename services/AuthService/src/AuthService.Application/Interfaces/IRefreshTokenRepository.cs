using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(Guid refreshTokenId, CancellationToken cancellationToken = default);
    Task RevokeActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
