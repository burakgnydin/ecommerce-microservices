using AuthService.Infrastructure.Security;

namespace AuthService.UnitTests.Security;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ReturnsValueDifferentFromPlainTextPassword()
    {
        var hash = _hasher.Hash("Sup3rSecret!");

        Assert.NotEqual("Sup3rSecret!", hash);
    }

    [Fact]
    public void Hash_ReturnsDifferentHashes_ForSamePasswordHashedTwice()
    {
        var hash1 = _hasher.Hash("Sup3rSecret!");
        var hash2 = _hasher.Hash("Sup3rSecret!");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_ReturnsTrue_WhenPasswordMatchesHash()
    {
        var hash = _hasher.Hash("Sup3rSecret!");

        Assert.True(_hasher.Verify("Sup3rSecret!", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPasswordDoesNotMatchHash()
    {
        var hash = _hasher.Hash("Sup3rSecret!");

        Assert.False(_hasher.Verify("WrongPassword", hash));
    }
}
