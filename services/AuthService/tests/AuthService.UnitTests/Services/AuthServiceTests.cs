using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Moq;
using SutAuthService = AuthService.Application.Services.AuthService;

namespace AuthService.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly SutAuthService _sut;

    public AuthServiceTests()
    {
        _sut = new SutAuthService(_userRepository.Object, _refreshTokenRepository.Object, _passwordHasher.Object, _tokenService.Object);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WhenEmailIsUnique()
    {
        var dto = new RegisterRequestDto("Jane Doe", "jane@example.com", "Sup3rSecret!");
        _userRepository.Setup(r => r.ExistsByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(dto.Password)).Returns("hashed-password");

        var result = await _sut.RegisterAsync(dto);

        Assert.Equal("Jane Doe", result.Name);
        Assert.Equal("jane@example.com", result.Email);
        Assert.Equal(Role.Customer, result.Role);
        _userRepository.Verify(r => r.CreateAsync(It.Is<User>(u => u.Email == dto.Email && u.PasswordHash == "hashed-password"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEmailAlreadyExists()
    {
        var dto = new RegisterRequestDto("Jane Doe", "jane@example.com", "Sup3rSecret!");
        _userRepository.Setup(r => r.ExistsByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateEmailException>(() => _sut.RegisterAsync(dto));
        _userRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokens_WhenCredentialsAreValid()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        var dto = new LoginRequestDto("jane@example.com", "Sup3rSecret!");
        _userRepository.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
        _tokenService.Setup(t => t.HashRefreshToken("refresh-token")).Returns("refresh-token-hash");
        _tokenService.Setup(t => t.AccessTokenLifetime).Returns(TimeSpan.FromMinutes(15));
        _tokenService.Setup(t => t.RefreshTokenLifetime).Returns(TimeSpan.FromDays(7));

        var result = await _sut.LoginAsync(dto);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(900, result.ExpiresInSeconds);
        _refreshTokenRepository.Verify(r => r.RevokeActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.Is<RefreshToken>(rt => rt.UserId == user.Id && rt.TokenHash == "refresh-token-hash"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenEmailDoesNotExist()
    {
        var dto = new LoginRequestDto("missing@example.com", "Sup3rSecret!");
        _userRepository.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(dto));
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenPasswordIsIncorrect()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        var dto = new LoginRequestDto("jane@example.com", "WrongPassword!");
        _userRepository.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(dto));
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_RotatesToken_WhenTokenIsActive()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        var existingToken = new RefreshToken(user.Id, "old-token-hash", DateTime.UtcNow.AddDays(1));
        var dto = new RefreshRequestDto("raw-refresh-token");
        _tokenService.Setup(t => t.HashRefreshToken("raw-refresh-token")).Returns("old-token-hash");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("old-token-hash", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("new-access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh-token");
        _tokenService.Setup(t => t.HashRefreshToken("new-refresh-token")).Returns("new-token-hash");
        _tokenService.Setup(t => t.AccessTokenLifetime).Returns(TimeSpan.FromMinutes(15));
        _tokenService.Setup(t => t.RefreshTokenLifetime).Returns(TimeSpan.FromDays(7));

        var result = await _sut.RefreshAsync(dto);

        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        _refreshTokenRepository.Verify(r => r.RevokeAsync(existingToken.Id, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.Is<RefreshToken>(rt => rt.UserId == user.Id && rt.TokenHash == "new-token-hash"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_Throws_WhenTokenNotFound()
    {
        var dto = new RefreshRequestDto("unknown-token");
        _tokenService.Setup(t => t.HashRefreshToken("unknown-token")).Returns("unknown-hash");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("unknown-hash", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.RefreshAsync(dto));
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_Throws_WhenTokenIsRevoked()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        var existingToken = new RefreshToken(user.Id, "old-token-hash", DateTime.UtcNow.AddDays(1));
        existingToken.Revoke();
        var dto = new RefreshRequestDto("raw-refresh-token");
        _tokenService.Setup(t => t.HashRefreshToken("raw-refresh-token")).Returns("old-token-hash");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("old-token-hash", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.RefreshAsync(dto));
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_RevokesToken_WhenTokenIsActive()
    {
        var existingToken = new RefreshToken(Guid.NewGuid(), "token-hash", DateTime.UtcNow.AddDays(1));
        var dto = new LogoutRequestDto("raw-refresh-token");
        _tokenService.Setup(t => t.HashRefreshToken("raw-refresh-token")).Returns("token-hash");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("token-hash", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

        await _sut.LogoutAsync(dto);

        _refreshTokenRepository.Verify(r => r.RevokeAsync(existingToken.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_IsIdempotent_WhenTokenNotFound()
    {
        var dto = new LogoutRequestDto("unknown-token");
        _tokenService.Setup(t => t.HashRefreshToken("unknown-token")).Returns("unknown-hash");
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("unknown-hash", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        await _sut.LogoutAsync(dto);

        _refreshTokenRepository.Verify(r => r.RevokeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
