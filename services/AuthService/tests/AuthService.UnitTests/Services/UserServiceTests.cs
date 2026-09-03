using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using Moq;

namespace AuthService.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository.Object, _passwordHasher.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(user.Id);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenUserDoesNotExist()
    {
        var id = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(id));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProfile_WhenEmailUnchanged()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.UpdateAsync(user.Id, new UserUpdateRequestDto("Jane Smith", "jane@example.com"));

        Assert.Equal("Jane Smith", result.Name);
        Assert.Equal("jane@example.com", result.Email);
        _userRepository.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProfile_WhenNewEmailIsAvailable()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepository.Setup(r => r.ExistsByEmailAsync("jane.smith@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.UpdateAsync(user.Id, new UserUpdateRequestDto("Jane Doe", "jane.smith@example.com"));

        Assert.Equal("jane.smith@example.com", result.Email);
        _userRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenUserDoesNotExist()
    {
        var id = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateAsync(id, new UserUpdateRequestDto("Jane Doe", "jane@example.com")));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNewEmailIsAlreadyTaken()
    {
        var user = new User("Jane Doe", "jane@example.com", "hashed-password");
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepository.Setup(r => r.ExistsByEmailAsync("taken@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => _sut.UpdateAsync(user.Id, new UserUpdateRequestDto("Jane Doe", "taken@example.com")));

        _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_ChangesPassword_WhenCurrentPasswordIsCorrect()
    {
        var user = new User("Jane Doe", "jane@example.com", "old-hash");
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("old-password", "old-hash")).Returns(true);
        _passwordHasher.Setup(h => h.Hash("new-password")).Returns("new-hash");

        await _sut.ChangePasswordAsync(user.Id, new ChangePasswordRequestDto("old-password", "new-password"));

        Assert.Equal("new-hash", user.PasswordHash);
        _userRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_Throws_WhenUserDoesNotExist()
    {
        var id = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.ChangePasswordAsync(id, new ChangePasswordRequestDto("old-password", "new-password")));
    }

    [Fact]
    public async Task ChangePasswordAsync_Throws_WhenCurrentPasswordIsIncorrect()
    {
        var user = new User("Jane Doe", "jane@example.com", "old-hash");
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("wrong-password", "old-hash")).Returns(false);

        await Assert.ThrowsAsync<InvalidCurrentPasswordException>(
            () => _sut.ChangePasswordAsync(user.Id, new ChangePasswordRequestDto("wrong-password", "new-password")));

        _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
