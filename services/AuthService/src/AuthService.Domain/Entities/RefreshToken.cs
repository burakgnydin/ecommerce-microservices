namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAt)
    {
        Validate(userId, tokenHash, expiresAt);

        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }

    private static void Validate(Guid userId, string tokenHash, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration must be in the future.", nameof(expiresAt));
    }
}
