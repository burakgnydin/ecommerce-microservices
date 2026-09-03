using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public string ShippingTitle { get; private set; }
    public string ShippingCity { get; private set; }
    public string ShippingDistrict { get; private set; }
    public string ShippingFullAddress { get; private set; }

    // Reserved for EF Core materialization: navigation collections cannot be bound
    // through the public constructor's parameters, only scalar properties can.
    private Order()
    {
    }

    public Order(
        Guid userId,
        IEnumerable<OrderItem> items,
        string shippingTitle,
        string shippingCity,
        string shippingDistrict,
        string shippingFullAddress)
    {
        var itemList = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        Validate(userId, itemList, shippingTitle, shippingCity, shippingDistrict, shippingFullAddress);

        Id = Guid.NewGuid();
        UserId = userId;
        Status = OrderStatus.Pending;
        Items = itemList;
        TotalAmount = itemList.Sum(i => i.Subtotal);
        ShippingTitle = shippingTitle;
        ShippingCity = shippingCity;
        ShippingDistrict = shippingDistrict;
        ShippingFullAddress = shippingFullAddress;
        CreatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Order cannot be cancelled while in '{Status}' status.");

        Status = OrderStatus.Cancelled;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Order cannot be marked as paid while in '{Status}' status.");

        Status = OrderStatus.Paid;
    }

    private static void Validate(
        Guid userId,
        ICollection<OrderItem> items,
        string shippingTitle,
        string shippingCity,
        string shippingDistrict,
        string shippingFullAddress)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (items.Count == 0)
            throw new ArgumentException("Order must contain at least one item.", nameof(items));
        if (string.IsNullOrWhiteSpace(shippingTitle))
            throw new ArgumentException("ShippingTitle is required.", nameof(shippingTitle));
        if (string.IsNullOrWhiteSpace(shippingCity))
            throw new ArgumentException("ShippingCity is required.", nameof(shippingCity));
        if (string.IsNullOrWhiteSpace(shippingDistrict))
            throw new ArgumentException("ShippingDistrict is required.", nameof(shippingDistrict));
        if (string.IsNullOrWhiteSpace(shippingFullAddress))
            throw new ArgumentException("ShippingFullAddress is required.", nameof(shippingFullAddress));
    }
}
