using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Application.Mapping;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
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

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        await _refreshTokenRepository.RevokeActiveByUserIdAsync(user.Id, cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<LoginResponseDto> RefreshAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(dto.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (existingToken is null || !existingToken.IsActive)
            throw new InvalidRefreshTokenException();

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
            throw new InvalidRefreshTokenException();

        await _refreshTokenRepository.RevokeAsync(existingToken.Id, cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(LogoutRequestDto dto, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(dto.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (existingToken is null || !existingToken.IsActive)
            return;

        await _refreshTokenRepository.RevokeAsync(existingToken.Id, cancellationToken);
    }

    private async Task<LoginResponseDto> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);
        var refreshTokenLifetime = _tokenService.RefreshTokenLifetime;

        var refreshTokenEntity = new RefreshToken(user.Id, refreshTokenHash, DateTime.UtcNow.Add(refreshTokenLifetime));
        await _refreshTokenRepository.CreateAsync(refreshTokenEntity, cancellationToken);

        return new LoginResponseDto(accessToken, refreshToken, (int)_tokenService.AccessTokenLifetime.TotalSeconds);
    }
}
