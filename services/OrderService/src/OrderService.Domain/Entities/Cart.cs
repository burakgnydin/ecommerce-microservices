namespace OrderService.Domain.Entities;

public class Cart
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ICollection<CartItem> Items { get; private set; } = new List<CartItem>();
    public DateTime UpdatedAt { get; private set; }

    // Reserved for EF Core materialization: navigation collections cannot be bound
    // through the public constructor's parameters, only scalar properties can.
    private Cart()
    {
    }

    public Cart(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        Id = Guid.NewGuid();
        UserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, int quantity)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is null)
        {
            Items.Add(new CartItem(productId, quantity));
        }
        else
        {
            existingItem.IncreaseQuantity(quantity);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        var item = Items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException($"Product '{productId}' is not in the cart.");

        item.SetQuantity(quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            return;
        }

        Items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
