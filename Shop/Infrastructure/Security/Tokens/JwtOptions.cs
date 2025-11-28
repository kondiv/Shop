namespace Shop.Infrastructure.Security.Tokens;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int LifetimeSec { get; set; }

    public string Key { get; set; } = string.Empty;
}
