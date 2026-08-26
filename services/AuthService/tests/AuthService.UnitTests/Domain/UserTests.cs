using AuthService.Domain.Entities;

namespace AuthService.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_CreatesUser_WhenDataIsValid()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");

        Assert.Equal("Jane Doe", user.Name);
        Assert.Equal("jane@example.com", user.Email);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal(Role.Customer, user.Role);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Constructor_AssignsGivenRole_WhenRoleIsProvided()
    {
        var user = new User("Admin User", "admin@example.com", "hashed-password", Role.Admin);

        Assert.Equal(Role.Admin, user.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenNameIsEmptyOrWhitespace(string? name)
    {
        Assert.Throws<ArgumentException>(() => new User(name!, "jane@example.com", "hashed-password"));
    }

    [Fact]
    public void Constructor_Throws_WhenNameExceedsMaxLength()
    {
        var name = new string('a', 201);

        Assert.Throws<ArgumentException>(() => new User(name, "jane@example.com", "hashed-password"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenEmailIsEmptyOrWhitespace(string? email)
    {
        Assert.Throws<ArgumentException>(() => new User("Jane Doe", email!, "hashed-password"));
    }

    [Fact]
    public void Constructor_Throws_WhenEmailHasNoAtSign()
    {
        Assert.Throws<ArgumentException>(() => new User("Jane Doe", "not-an-email", "hashed-password"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_Throws_WhenPasswordHashIsEmptyOrWhitespace(string? passwordHash)
    {
        Assert.Throws<ArgumentException>(() => new User("Jane Doe", "jane@example.com", passwordHash!));
    }
}
