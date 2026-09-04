namespace AuthService.Application.DTOs;

public record TokenResponseDto(string AccessToken, int ExpiresInSeconds);
