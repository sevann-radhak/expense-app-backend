namespace ExpenseTracker.Api.Configuration;

public sealed class OpenApiOptions
{
    public const string SectionName = "OpenApi";

    public string DocumentName { get; set; } = "v1";

    public string Title { get; set; } = "Expense Tracker API";

    public string Version { get; set; } = "v1";

    public string Description { get; set; } =
        "Expense tracker API: auth (JWT), users, dev book seeding, health.";

    public string SecuritySchemeId { get; set; } = "bearer";

    public string SecuritySchemeDescription { get; set; } =
        "JWT Authorization header using the Bearer scheme.";

    public string SwaggerJsonPath { get; set; } = "/swagger/v1/swagger.json";

    public string SwaggerUiDocumentTitle { get; set; } = "Expense Tracker API v1";
}
