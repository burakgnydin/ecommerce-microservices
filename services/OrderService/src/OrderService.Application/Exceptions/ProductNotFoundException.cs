namespace OrderService.Application.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId)
        : base($"Product '{productId}' was not found.")
    {
    }
}
