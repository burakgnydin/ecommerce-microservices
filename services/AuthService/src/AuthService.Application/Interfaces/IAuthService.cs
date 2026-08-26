using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default);
}
