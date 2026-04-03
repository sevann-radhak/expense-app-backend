using System.Threading.RateLimiting;

namespace ExpenseTracker.Api.Hosting;

public static class RateLimiterExtensions
{
    public const string AuthPolicy = "auth-ip";
    public const string SyncPolicy = "sync-user";

    public static IServiceCollection AddExpenseTrackerRateLimiter(this IServiceCollection services, IHostEnvironment environment)
    {
        return environment.IsEnvironment("Integration")
            ? services
            : services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            _ = options.AddPolicy(
                AuthPolicy,
                httpContext =>
                {
                    string partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 120,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true,
                        });
                });
            _ = options.AddPolicy(
                SyncPolicy,
                httpContext =>
                {
                    string? userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    string partitionKey = string.IsNullOrEmpty(userId) ? "anon" : userId;
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 300,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true,
                        });
                });
        });
    }

    public static IApplicationBuilder UseExpenseTrackerRateLimiter(this WebApplication app, IHostEnvironment environment)
    {
        return environment.IsEnvironment("Integration") ? app : app.UseRateLimiter();
    }
}
