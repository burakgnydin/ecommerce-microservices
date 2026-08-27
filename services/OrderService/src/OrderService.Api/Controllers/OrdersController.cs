using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Api.Controllers;

/// <summary>
/// Manages the authenticated user's own orders.
/// </summary>
[ApiController]
[Route("api/orders")]
[Tags("Orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Returns the authenticated user's orders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _orderService.GetByUserIdAsync(GetUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single order owned by the authenticated user.
    /// </summary>
    /// <param name="id">Order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new order for the authenticated user.
    /// </summary>
    /// <param name="dto">Order data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType<OrderResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponseDto>> Create(OrderCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateAsync(GetUserId(), dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates the status of an order owned by the authenticated user. Supports transitioning
    /// to "Cancelled" or "Paid".
    /// </summary>
    /// <param name="id">Order id.</param>
    /// <param name="dto">Requested status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType<OrderResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(Guid id, OrderStatusUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = dto.Status switch
        {
            "Cancelled" => await _orderService.CancelAsync(id, GetUserId(), cancellationToken),
            "Paid" => await _orderService.MarkAsPaidAsync(id, GetUserId(), cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto.Status, "Unsupported order status.")
        };

        return Ok(result);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
