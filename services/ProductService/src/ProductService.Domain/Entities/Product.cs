namespace ProductService.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Product(string name, string? description, decimal price, int stock, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters.", nameof(name));
        if (description is { Length: > 2000 })
            throw new ArgumentException("Product description cannot exceed 2000 characters.", nameof(description));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Product price cannot be negative.");
        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), stock, "Product stock cannot be negative.");

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
    }
}
