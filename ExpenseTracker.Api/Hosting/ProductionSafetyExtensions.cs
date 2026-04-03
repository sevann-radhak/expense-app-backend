using ExpenseTracker.Api.Configuration;

namespace ExpenseTracker.Api.Hosting;

public static class ProductionSafetyExtensions
{
    /// <summary>
    /// Fails fast when production would expose dev-only book endpoints.
    /// </summary>
    public static void ValidateProductionSafety(this WebApplication app)
    {
        if (!app.Environment.IsProduction())
        {
            return;
        }

        DevDataOptions dev = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DevDataOptions>>().Value;
        if (dev.ExposeEndpoints)
        {
            throw new InvalidOperationException(
                "DevData:ExposeEndpoints must be false in Production. Remove or override in appsettings.Production.json / environment variables.");
        }
    }
}
