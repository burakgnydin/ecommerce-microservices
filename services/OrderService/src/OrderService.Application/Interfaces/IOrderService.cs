using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateAsync(Guid userId, OrderCreateDto dto, CancellationToken cancellationToken = default);

    Task<OrderResponseDto> GetByIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<OrderResponseDto> CancelAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);
}
