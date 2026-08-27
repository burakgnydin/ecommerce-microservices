namespace OrderService.Application.DTOs;

/// <summary>
/// Payload used to update an order's status.
/// </summary>
/// <param name="Status">Requested status. Only "Cancelled" is supported in v1.</param>
public record OrderStatusUpdateDto(string Status);
