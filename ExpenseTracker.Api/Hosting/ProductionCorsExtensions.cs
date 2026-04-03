using ExpenseTracker.Api.Configuration;

namespace ExpenseTracker.Api.Hosting;

public static class ProductionCorsExtensions
{
    public static void ValidateProductionCors(this WebApplication app)
    {
        if (!app.Environment.IsProduction())
        {
            return;
        }

        AppCorsOptions cors = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppCorsOptions>>().Value;
        if (cors.AllowAnyOrigin)
        {
            return;
        }

        bool hasOrigin = cors.AllowedOrigins is { Length: > 0 } &&
                         cors.AllowedOrigins.Any(o => !string.IsNullOrWhiteSpace(o));
        if (!hasOrigin)
        {
            throw new InvalidOperationException(
                "Production: set Cors:AllowedOrigins (explicit SPA URLs) or set Cors:AllowAnyOrigin to true (not recommended for public APIs).");
        }
    }
}
