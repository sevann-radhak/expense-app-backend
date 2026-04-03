using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.IntegrationTests.Support;

public sealed class ExpenseTrackerApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ExpenseTrackerApiFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _ = builder.UseEnvironment("Integration");
        _ = builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = _connectionString,
                        ["Jwt:SigningKey"] = "Integration-Tests-Jwt-Signing-Key-32!",
                        ["Jwt:Issuer"] = "ExpenseTrackerTests",
                        ["Jwt:Audience"] = "ExpenseTrackerTests",
                        ["InitialAdmin:Enabled"] = "false",
                        ["Setup:BootstrapToken"] = string.Empty,
                        ["DevData:ExposeEndpoints"] = "true",
                        ["DevData:RequireSharedSecret"] = "false",
                        ["Api:LogWhenDatabaseDisabled"] = "false",
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
                    });
            });
    }
}
