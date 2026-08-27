namespace OrderService.Application.Exceptions;

public class ProductServiceUnavailableException : Exception
{
    public ProductServiceUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
