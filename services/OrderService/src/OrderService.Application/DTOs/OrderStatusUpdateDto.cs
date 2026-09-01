namespace OrderService.Application.DTOs;

/// <summary>
/// Payload used to update an order's status.
/// </summary>
/// <param name="Status">Requested status. Supports "Cancelled".</param>
public record OrderStatusUpdateDto(string Status);
