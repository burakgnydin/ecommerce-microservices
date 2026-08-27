using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }

    // Only a masked representation (e.g. "**** **** **** 1234") is ever stored;
    // the raw card number is never persisted or logged.
    public string MaskedCardNumber { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Reserved for EF Core materialization.
    private Payment()
    {
    }

    private Payment(Guid orderId, Guid userId, decimal amount, string maskedCardNumber, PaymentStatus status)
    {
        Validate(orderId, userId, amount, maskedCardNumber);

        Id = Guid.NewGuid();
        OrderId = orderId;
        UserId = userId;
        Amount = amount;
        MaskedCardNumber = maskedCardNumber;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public static Payment Succeeded(Guid orderId, Guid userId, decimal amount, string maskedCardNumber)
        => new(orderId, userId, amount, maskedCardNumber, PaymentStatus.Succeeded);

    public static Payment Failed(Guid orderId, Guid userId, decimal amount, string maskedCardNumber)
        => new(orderId, userId, amount, maskedCardNumber, PaymentStatus.Failed);

    private static void Validate(Guid orderId, Guid userId, decimal amount, string maskedCardNumber)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId is required.", nameof(orderId));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        if (string.IsNullOrWhiteSpace(maskedCardNumber))
            throw new ArgumentException("MaskedCardNumber is required.", nameof(maskedCardNumber));
    }
}
