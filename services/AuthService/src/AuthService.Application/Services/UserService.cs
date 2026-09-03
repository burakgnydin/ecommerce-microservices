using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Application.Mapping;

namespace AuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User with id '{id}' was not found.");

        return user.ToDto();
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(u => u.ToDto()).ToList();
    }

    public async Task<UserResponseDto> UpdateAsync(Guid id, UserUpdateRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User with id '{id}' was not found.");

        if (!string.Equals(user.Email, dto.Email, StringComparison.Ordinal) &&
            await _userRepository.ExistsByEmailAsync(dto.Email, cancellationToken))
        {
            throw new DuplicateEmailException(dto.Email);
        }

        user.UpdateProfile(dto.Name, dto.Email);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return user.ToDto();
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User with id '{id}' was not found.");

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new InvalidCurrentPasswordException();

        user.ChangePassword(_passwordHasher.Hash(dto.NewPassword));
        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
