using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Application.Mapping;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.ExistsByEmailAsync(dto.Email, cancellationToken))
            throw new DuplicateEmailException(dto.Email);

        var passwordHash = _passwordHasher.Hash(dto.Password);
        var user = new User(dto.Name, dto.Email, passwordHash);

        await _userRepository.CreateAsync(user, cancellationToken);

        return user.ToDto();
    }
}
