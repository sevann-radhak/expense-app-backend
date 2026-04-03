using ExpenseTracker.IntegrationTests.Fixtures;
using System.Net;
using System.Text.Json;

namespace ExpenseTracker.IntegrationTests.Api;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Api")]
public sealed class PublicApiTests(IntegrationHostFixture host)
{
    [Fact]
    public async Task GetHealth_ReturnsOkPayload()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(new Uri("/api/health", UriKind.Relative)).ConfigureAwait(false);

        _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        _ = body.Should().Contain("expense-tracker-api");
    }

    [Fact]
    public async Task GetHello_ReturnsMessage()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(new Uri("/api/hello", UriKind.Relative)).ConfigureAwait(false);

        _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        _ = doc.RootElement.GetProperty("message").GetString().Should().NotBeNullOrEmpty();
    }
}
