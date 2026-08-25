namespace ProductService.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Category name cannot exceed 100 characters.", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
    }
}
