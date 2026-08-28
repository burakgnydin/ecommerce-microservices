namespace PaymentService.Application.Exceptions;

public class OrderNotPayableException : Exception
{
    public OrderNotPayableException(string message) : base(message)
    {
    }
}
