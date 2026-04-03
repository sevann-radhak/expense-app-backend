namespace ExpenseTracker.Api.Configuration;

public sealed class AppCorsOptions
{
    public const string SectionName = "Cors";

    public bool AllowAnyOrigin { get; set; } = true;

    public string[] AllowedOrigins { get; set; } = [];
}
