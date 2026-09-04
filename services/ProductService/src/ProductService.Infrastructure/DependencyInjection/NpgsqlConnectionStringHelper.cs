namespace ProductService.Infrastructure.DependencyInjection;

/// <summary>
/// Render's managed Postgres exposes connection info as a postgres:// URI, but Npgsql's
/// connection string builder only understands ADO-style Host=...;Port=... pairs. Converts
/// when a URI is detected; passes through unchanged for local dev's Host=... strings.
/// </summary>
public static class NpgsqlConnectionStringHelper
{
    public static string? Normalize(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString) ||
            !connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
               $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
}
