namespace ExpenseTracker.Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "ExpenseTracker";

    public string Audience { get; set; } = "ExpenseTracker";

    /// <summary>HMAC-SHA256 key; use at least 32 bytes (256 bits). Set via user secrets or environment variables.</summary>
    public string SigningKey { get; set; } = "";

    public int AccessTokenMinutes { get; set; } = 120;
}
