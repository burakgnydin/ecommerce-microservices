using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.Api.Controllers;

/// <summary>
/// Charges the authenticated user's own orders.
/// </summary>
[ApiController]
[Route("api/payments")]
[Tags("Payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Returns the authenticated user's own payment history.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PaymentResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetByUserIdAsync(GetUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns every payment across all users, most recent first. Restricted to admins.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<IReadOnlyList<PaymentResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentResponseDto>>> GetAllForAdmin(CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Charges a card against one of the authenticated user's own orders.
    /// </summary>
    /// <param name="dto">Payment data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType<PaymentResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentResponseDto>> Charge(PaymentRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _paymentService.ChargeAsync(GetUserId(), dto, GetBearerToken(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    private string GetBearerToken() => Request.Headers.Authorization.ToString()["Bearer ".Length..];
}
