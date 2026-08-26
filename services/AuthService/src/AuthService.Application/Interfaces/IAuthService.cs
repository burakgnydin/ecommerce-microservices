using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequestDto dto, CancellationToken cancellationToken = default);
}
