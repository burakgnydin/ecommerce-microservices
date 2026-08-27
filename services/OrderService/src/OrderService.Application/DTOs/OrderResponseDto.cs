namespace OrderService.Application.DTOs;

/// <summary>
/// Represents an order as returned by the API.
/// </summary>
/// <param name="Id">Unique order identifier.</param>
/// <param name="UserId">Id of the user who placed the order.</param>
/// <param name="Status">Order status: Pending, Paid or Cancelled.</param>
/// <param name="TotalAmount">Sum of all order item subtotals.</param>
/// <param name="CreatedAt">Date and time the order was created, in UTC.</param>
/// <param name="Items">Ordered products.</param>
public record OrderResponseDto(
    Guid Id,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemResponseDto> Items);
