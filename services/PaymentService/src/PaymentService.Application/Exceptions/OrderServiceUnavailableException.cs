namespace PaymentService.Application.Exceptions;

public class OrderServiceUnavailableException : Exception
{
    public OrderServiceUnavailableException(string message) : base(message)
    {
    }

    public OrderServiceUnavailableException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
