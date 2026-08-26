using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthService.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef` create migrations without a startup project (AuthService.Api
/// does not exist yet). Reads the connection string from an environment variable
/// only, never a hardcoded value, to keep secrets out of source control.
/// </summary>
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__AuthDb")
            ?? throw new InvalidOperationException(
                "Set the ConnectionStrings__AuthDb environment variable before running EF Core design-time commands.");

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AuthDbContext(optionsBuilder.Options);
    }
}
