using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService.Api.Controllers;

/// <summary>
/// Handles user registration and authentication.
/// </summary>
[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IHostEnvironment _environment;

    public AuthController(IAuthService authService, ITokenService tokenService, IHostEnvironment environment)
    {
        _authService = authService;
        _tokenService = tokenService;
        _environment = environment;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="dto">Registration data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("register")]
    [EnableRateLimiting("register")]
    [ProducesResponseType<UserResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserResponseDto>> Register(RegisterRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Register), new { }, result);
    }

    /// <summary>
    /// Authenticates a user and issues an access token, setting the refresh token as an HttpOnly cookie.
    /// </summary>
    /// <param name="dto">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType<TokenResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TokenResponseDto>> Login(LoginRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new TokenResponseDto(result.AccessToken, result.ExpiresInSeconds));
    }

    /// <summary>
    /// Rotates the refresh token cookie, issuing a new access token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("refresh")]
    [ProducesResponseType<TokenResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponseDto>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        var result = await _authService.RefreshAsync(new RefreshRequestDto(refreshToken), cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new TokenResponseDto(result.AccessToken, result.ExpiresInSeconds));
    }

    /// <summary>
    /// Revokes the refresh token and clears its cookie.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
        {
            await _authService.LogoutAsync(new LogoutRequestDto(refreshToken), cancellationToken);
        }

        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = "/" });
        return NoContent();
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.Add(_tokenService.RefreshTokenLifetime),
        });
    }
}
