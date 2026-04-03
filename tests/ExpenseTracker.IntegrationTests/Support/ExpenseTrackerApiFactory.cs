using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.IntegrationTests.Support;

public sealed class ExpenseTrackerApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly IReadOnlyDictionary<string, string?>? _configurationOverrides;
    private readonly string? _environmentName;

    public ExpenseTrackerApiFactory(
        string connectionString,
        IReadOnlyDictionary<string, string?>? configurationOverrides = null,
        string? environmentName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
        _configurationOverrides = configurationOverrides;
        _environmentName = environmentName;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Dictionary<string, string?> settings = new()
        {
            ["ConnectionStrings:DefaultConnection"] = _connectionString,
            ["Jwt:SigningKey"] = "Integration-Tests-Jwt-Signing-Key-32!",
            ["Jwt:Issuer"] = "ExpenseTrackerTests",
            ["Jwt:Audience"] = "ExpenseTrackerTests",
            ["InitialAdmin:Enabled"] = "false",
            ["Setup:BootstrapToken"] = IntegrationTestConstants.BootstrapToken,
            ["DevData:ExposeEndpoints"] = "true",
            ["DevData:RequireSharedSecret"] = "false",
            ["DevData:SharedSecret"] = string.Empty,
            ["Api:LogWhenDatabaseDisabled"] = "false",
            ["Logging:LogLevel:Default"] = "Warning",
            ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
            ["Cors:AllowAnyOrigin"] = "true",
        };

        if (_configurationOverrides is not null)
        {
            foreach (KeyValuePair<string, string?> pair in _configurationOverrides)
            {
                settings[pair.Key] = pair.Value;
            }
        }

        if (!string.IsNullOrEmpty(_environmentName))
        {
            _ = builder.UseEnvironment(_environmentName);
        }
        else
        {
            _ = builder.UseEnvironment("Integration");
        }

        _ = builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                config.AddInMemoryCollection(settings);
            });
    }
}
