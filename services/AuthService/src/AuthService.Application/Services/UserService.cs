using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Application.Mapping;

namespace AuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            throw new NotFoundException($"User with id '{id}' was not found.");

        return user.ToDto();
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
}
