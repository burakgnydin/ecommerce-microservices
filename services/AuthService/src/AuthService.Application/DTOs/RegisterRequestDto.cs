namespace AuthService.Application.DTOs;

/// <summary>
/// Payload used to register a new user.
/// </summary>
/// <param name="Name">User's display name (max 200 characters).</param>
/// <param name="Email">User's email address, must be unique.</param>
/// <param name="Password">Plain-text password; hashed before storage.</param>
public record RegisterRequestDto(string Name, string Email, string Password);
