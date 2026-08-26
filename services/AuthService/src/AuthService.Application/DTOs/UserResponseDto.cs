using AuthService.Domain.Entities;

namespace AuthService.Application.DTOs;

/// <summary>
/// Represents a user returned by the API.
/// </summary>
/// <param name="Id">Unique user identifier.</param>
/// <param name="Name">User's display name.</param>
/// <param name="Email">User's email address.</param>
/// <param name="Role">User's role.</param>
/// <param name="CreatedAt">When the user account was created.</param>
public record UserResponseDto(
    Guid Id,
    string Name,
    string Email,
    Role Role,
    DateTime CreatedAt);
