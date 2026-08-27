namespace OrderService.Application.DTOs;

/// <summary>
/// Represents a user's cart as returned by the API.
/// </summary>
/// <param name="Id">Unique cart identifier.</param>
/// <param name="UserId">Id of the user who owns the cart.</param>
/// <param name="Items">Products currently in the cart.</param>
/// <param name="UpdatedAt">Date and time the cart was last modified, in UTC.</param>
public record CartResponseDto(
    Guid Id,
    Guid UserId,
    IReadOnlyList<CartItemResponseDto> Items,
    DateTime UpdatedAt);
