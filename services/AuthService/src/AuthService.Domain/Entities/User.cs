namespace AuthService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Role Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User(string name, string email, string passwordHash, Role role = Role.Customer)
    {
        Validate(name, email, passwordHash);

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    private static void Validate(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("User name is required.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("User name cannot exceed 200 characters.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (email.Length > 320)
            throw new ArgumentException("Email cannot exceed 320 characters.", nameof(email));
        if (!email.Contains('@'))
            throw new ArgumentException("Email must be a valid email address.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
    }
}
