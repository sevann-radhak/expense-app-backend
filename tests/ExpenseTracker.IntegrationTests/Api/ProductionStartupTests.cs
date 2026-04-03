using ExpenseTracker.IntegrationTests.Fixtures;
using ExpenseTracker.IntegrationTests.Support;
using System.Net;

namespace ExpenseTracker.IntegrationTests.Api;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Api")]
public sealed class ProductionStartupTests(IntegrationHostFixture host)
{
    [Fact]
    public void CreateHost_ProductionWithDevEndpointsEnabled_ThrowsInvalidOperation()
    {
        ExpenseTrackerApiFactory factory = new(
            host.ConnectionString,
            new Dictionary<string, string?>
            {
                ["DevData:ExposeEndpoints"] = "true",
                ["Cors:AllowAnyOrigin"] = "false",
                ["Cors:AllowedOrigins:0"] = "https://localhost",
            },
            "Production");

        Action act = () => _ = factory.CreateClient();
        _ = act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("DevData");
        factory.Dispose();
    }

    [Fact]
    public void CreateHost_ProductionWithEmptyCorsAllowlist_ThrowsInvalidOperation()
    {
        Dictionary<string, string?> overrides = new()
        {
            ["DevData:ExposeEndpoints"] = "false",
            ["Cors:AllowAnyOrigin"] = "false",
        };

        for (int i = 0; i < 16; i++)
        {
            overrides[$"Cors:AllowedOrigins:{i}"] = string.Empty;
        }

        ExpenseTrackerApiFactory factory = new(host.ConnectionString, overrides, "Production");

        Action act = () => _ = factory.CreateClient();
        _ = act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("Cors");
        factory.Dispose();
    }

    [Fact]
    public async Task Production_WithSafeCorsAndNoDevEndpoints_StartsAndHealthOk()
    {
        ExpenseTrackerApiFactory factory = new(
            host.ConnectionString,
            new Dictionary<string, string?>
            {
                ["DevData:ExposeEndpoints"] = "false",
                ["Cors:AllowAnyOrigin"] = "false",
                ["Cors:AllowedOrigins:0"] = "https://app.example.test",
            },
            "Production");

        HttpClient client = factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/api/health", UriKind.Relative)).ConfigureAwait(false);
        _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.Dispose();
    }
}
