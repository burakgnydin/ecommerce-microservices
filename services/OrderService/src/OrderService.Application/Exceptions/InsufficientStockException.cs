namespace OrderService.Application.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(Guid productId, int requestedQuantity, int availableStock)
        : base($"Product '{productId}' has insufficient stock. Requested {requestedQuantity}, available {availableStock}.")
    {
    }
}
