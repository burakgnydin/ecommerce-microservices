using AuthService.Application.DTOs;
using AuthService.Domain.Entities;

namespace AuthService.Application.Mapping;

public static class UserMapper
{
    public static UserResponseDto ToDto(this User user)
    {
        return new UserResponseDto(user.Id, user.Name, user.Email, user.Role, user.CreatedAt);
    }
}
