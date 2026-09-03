using System.IdentityModel.Tokens.Jwt;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

/// <summary>
/// Manages the authenticated user's own saved addresses.
/// </summary>
[ApiController]
[Route("api/users/me/addresses")]
[Tags("Addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    /// <summary>
    /// Returns the authenticated user's saved addresses.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AddressResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AddressResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _addressService.GetByUserIdAsync(GetUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Adds a new saved address for the authenticated user.
    /// </summary>
    /// <param name="dto">Address data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType<AddressResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddressResponseDto>> Create(AddressCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _addressService.CreateAsync(GetUserId(), dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), result);
    }

    /// <summary>
    /// Updates a saved address owned by the authenticated user.
    /// </summary>
    /// <param name="id">Address id.</param>
    /// <param name="dto">Updated address data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<AddressResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressResponseDto>> Update(Guid id, AddressUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _addressService.UpdateAsync(GetUserId(), id, dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a saved address owned by the authenticated user.
    /// </summary>
    /// <param name="id">Address id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _addressService.DeleteAsync(GetUserId(), id, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
