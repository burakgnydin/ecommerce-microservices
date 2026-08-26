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
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly SutAuthService _sut;

    public AuthServiceTests()
    {
        _sut = new SutAuthService(_userRepository.Object, _passwordHasher.Object);
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
}
