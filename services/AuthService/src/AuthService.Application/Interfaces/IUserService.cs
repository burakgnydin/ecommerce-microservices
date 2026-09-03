using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every user, ordered by name. Restricted to admins.
    /// </summary>
    Task<IReadOnlyList<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponseDto> UpdateAsync(Guid id, UserUpdateRequestDto dto, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid id, ChangePasswordRequestDto dto, CancellationToken cancellationToken = default);
}
