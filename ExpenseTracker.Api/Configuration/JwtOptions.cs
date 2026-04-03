namespace ExpenseTracker.Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "ExpenseTracker";

    public string Audience { get; set; } = "ExpenseTracker";

    public string SigningKey { get; set; } = "";

    public int AccessTokenMinutes { get; set; } = 120;

    public int MinimumSigningKeyLength { get; set; } = 32;

    public string DevelopmentFallbackSigningKey { get; set; } = "";

    public int ClockSkewMinutes { get; set; } = 1;
}
