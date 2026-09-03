namespace AuthService.Domain.Entities;

public class Address
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string City { get; private set; }
    public string District { get; private set; }
    public string FullAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Address(Guid userId, string title, string city, string district, string fullAddress)
    {
        Validate(userId, title, city, district, fullAddress);

        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        City = city;
        District = district;
        FullAddress = fullAddress;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string city, string district, string fullAddress)
    {
        Validate(UserId, title, city, district, fullAddress);

        Title = title;
        City = city;
        District = district;
        FullAddress = fullAddress;
    }

    private static void Validate(Guid userId, string title, string city, string district, string fullAddress)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (title.Length > 100)
            throw new ArgumentException("Title cannot exceed 100 characters.", nameof(title));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));
        if (city.Length > 100)
            throw new ArgumentException("City cannot exceed 100 characters.", nameof(city));
        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District is required.", nameof(district));
        if (district.Length > 100)
            throw new ArgumentException("District cannot exceed 100 characters.", nameof(district));
        if (string.IsNullOrWhiteSpace(fullAddress))
            throw new ArgumentException("FullAddress is required.", nameof(fullAddress));
        if (fullAddress.Length > 500)
            throw new ArgumentException("FullAddress cannot exceed 500 characters.", nameof(fullAddress));
    }
}
