namespace OrderService.Domain.Entities;

public class CartItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    // Reserved for EF Core materialization: navigation collections cannot be bound
    // through the public constructor's parameters, only scalar properties can.
    private CartItem()
    {
    }

    public CartItem(Guid productId, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
    }

    internal void IncreaseQuantity(int amount) => Quantity += amount;

    internal void SetQuantity(int quantity) => Quantity = quantity;
}
