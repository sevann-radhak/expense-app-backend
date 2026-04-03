namespace ExpenseTracker.Api.Configuration;

public sealed class ApiEndpointsOptions
{
    public const string SectionName = "Api";

    public string HealthStatus { get; set; } = "ok";

    public string HealthServiceName { get; set; } = "expense-tracker-api";

    public string HelloMessage { get; set; } = "Hello, world!";

    public bool LogWhenDatabaseDisabled { get; set; } = true;
}
