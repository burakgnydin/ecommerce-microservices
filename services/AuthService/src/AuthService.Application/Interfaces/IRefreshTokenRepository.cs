using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
