using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateAsync(Guid userId, OrderCreateDto dto, CancellationToken cancellationToken = default);

    Task<OrderResponseDto> GetByIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every order across all users, most recent first. Restricted to admins.
    /// </summary>
    Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<OrderResponseDto> CancelAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an order as paid. Only callable by trusted services (e.g. payment-service after a
    /// successful charge) — not scoped to a user, since the caller isn't the order's owner.
    /// </summary>
    Task<OrderResponseDto> MarkAsPaidAsync(Guid orderId, CancellationToken cancellationToken = default);
}
