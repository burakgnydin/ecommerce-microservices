using System.IdentityModel.Tokens.Jwt;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

/// <summary>
/// Exposes the authenticated user's own profile.
/// </summary>
[ApiController]
[Route("api/users")]
[Tags("Users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("me")]
    [ProducesResponseType<UserResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var result = await _userService.GetByIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns every user, ordered by name. Restricted to admins.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<IReadOnlyList<UserResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetAllForAdmin(CancellationToken cancellationToken)
    {
        var result = await _userService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the profile of the currently authenticated user.
    /// </summary>
    /// <param name="dto">The updated name and email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("me")]
    [ProducesResponseType<UserResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponseDto>> UpdateCurrentUser(UserUpdateRequestDto dto, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var result = await _userService.UpdateAsync(userId, dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
    /// <param name="dto">The current and new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        await _userService.ChangePasswordAsync(userId, dto, cancellationToken);
        return NoContent();
    }
}
