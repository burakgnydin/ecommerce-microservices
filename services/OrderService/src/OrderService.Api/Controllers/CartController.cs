using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Api.Controllers;

/// <summary>
/// Manages the authenticated user's own cart.
/// </summary>
[ApiController]
[Route("api/cart")]
[Tags("Cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Returns the authenticated user's cart, creating an empty one if it doesn't exist yet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType<CartResponseDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CartResponseDto>> Get(CancellationToken cancellationToken)
    {
        var result = await _cartService.GetOrCreateAsync(GetUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Adds a product to the cart, or increases its quantity if already present.
    /// </summary>
    /// <param name="dto">Product and quantity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("items")]
    [ProducesResponseType<CartResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartResponseDto>> AddItem(CartItemAddDto dto, CancellationToken cancellationToken)
    {
        var result = await _cartService.AddItemAsync(GetUserId(), dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Sets the quantity of a product already in the cart.
    /// </summary>
    /// <param name="productId">Id of the product to update.</param>
    /// <param name="dto">New quantity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType<CartResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponseDto>> UpdateItemQuantity(Guid productId, CartItemQuantityUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _cartService.UpdateItemQuantityAsync(GetUserId(), productId, dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes a product from the cart.
    /// </summary>
    /// <param name="productId">Id of the product to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType<CartResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponseDto>> RemoveItem(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _cartService.RemoveItemAsync(GetUserId(), productId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Converts the authenticated user's cart into an order, then clears the cart.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("checkout")]
    [ProducesResponseType<OrderResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> Checkout(CancellationToken cancellationToken)
    {
        var result = await _cartService.CheckoutAsync(GetUserId(), cancellationToken);
        return CreatedAtAction("GetById", "Orders", new { id = result.Id }, result);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
