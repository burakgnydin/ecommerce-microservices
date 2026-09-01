namespace PaymentService.Infrastructure.ExternalServices;

public class OrderServiceOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Long-lived JWT with a "Service" role claim, used to authenticate payment-service itself
    /// (not the calling user) when marking an order as paid.
    /// </summary>
    public string ServiceToken { get; set; } = string.Empty;
}
