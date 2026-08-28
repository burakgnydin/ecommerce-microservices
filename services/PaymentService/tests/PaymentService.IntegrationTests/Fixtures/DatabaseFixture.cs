using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PaymentService.IntegrationTests.Fixtures;

/// <summary>
/// Starts a disposable PostgreSQL container (via Testcontainers) for the duration of the test run
/// and applies the real EF Core migrations, so repository/API tests exercise the actual schema
/// instead of an in-memory provider.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("payment_service_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new PaymentDbContext(options);
    }
}
