namespace AuthService.Infrastructure.Security;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string PrivateKeyPem { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
}
