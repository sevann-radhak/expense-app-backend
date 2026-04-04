using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.IntegrationTests.Fixtures;
using ExpenseTracker.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace ExpenseTracker.IntegrationTests.Api;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Api")]
[Trait("Category", "Integration.Database")]
public sealed class DevBookApiTests(IntegrationHostFixture host)
{
    [Fact]
    public async Task Reset_WithoutUserId_ReturnsBadRequest()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();

        HttpResponseMessage res = await client
            .PostAsJsonAsync(new Uri("/api/dev/books/reset", UriKind.Relative), new { userId = (string?)null })
            .ConfigureAwait(false);

        _ = res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SeedTaxonomy_UnknownUserId_ReturnsNotFound()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();

        HttpResponseMessage res = await client
            .PostAsJsonAsync(
                new Uri("/api/dev/books/seed-taxonomy", UriKind.Relative),
                new { userId = "00000000-0000-0000-0000-000000000001" })
            .ConfigureAwait(false);

        _ = res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SeedTaxonomy_Twice_SecondReturnsConflict()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string userId = await RegisterUserIdAsync(client).ConfigureAwait(false);

        HttpResponseMessage first = await client
            .PostAsJsonAsync(new Uri("/api/dev/books/seed-taxonomy", UriKind.Relative), new { userId })
            .ConfigureAwait(false);
        _ = first.EnsureSuccessStatusCode();

        HttpResponseMessage second = await client
            .PostAsJsonAsync(new Uri("/api/dev/books/seed-taxonomy", UriKind.Relative), new { userId })
            .ConfigureAwait(false);
        _ = second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task SeedDemo_PersistsExpensesAndIncome()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string userId = await RegisterUserIdAsync(client).ConfigureAwait(false);

        HttpResponseMessage res = await client
            .PostAsJsonAsync(new Uri("/api/dev/books/seed-demo", UriKind.Relative), new { userId })
            .ConfigureAwait(false);
        _ = res.EnsureSuccessStatusCode();

        using IServiceScope scope = host.Factory.Services.CreateScope();
        ExpenseTrackerDbContext db = scope.ServiceProvider.GetRequiredService<ExpenseTrackerDbContext>();
        int expenseCount = await db.Expenses.CountAsync(e => e.UserId == userId).ConfigureAwait(false);
        int incomeCount = await db.IncomeEntries.CountAsync(e => e.UserId == userId).ConfigureAwait(false);
        _ = expenseCount.Should().BeGreaterThan(100);
        _ = incomeCount.Should().BeGreaterThan(20);
    }

    [Fact]
    public async Task Reset_ClearsBookRows()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string userId = await RegisterUserIdAsync(client).ConfigureAwait(false);

        HttpResponseMessage seed = await client
            .PostAsJsonAsync(new Uri("/api/dev/books/seed-demo", UriKind.Relative), new { userId })
            .ConfigureAwait(false);
        _ = seed.EnsureSuccessStatusCode();

        HttpResponseMessage reset = await client
            .PostAsJsonAsync(new Uri("/api/dev/books/reset", UriKind.Relative), new { userId })
            .ConfigureAwait(false);
        _ = reset.EnsureSuccessStatusCode();

        using IServiceScope scope = host.Factory.Services.CreateScope();
        ExpenseTrackerDbContext db = scope.ServiceProvider.GetRequiredService<ExpenseTrackerDbContext>();
        _ = (await db.Expenses.CountAsync(e => e.UserId == userId).ConfigureAwait(false)).Should().Be(0);
    }

    [Fact]
    public async Task DevBook_WithRequiredSecret_MissingHeader_ReturnsUnauthorized()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        using ExpenseTrackerApiFactory factory = new(
            host.ConnectionString,
            new Dictionary<string, string?>
            {
                ["DevData:RequireSharedSecret"] = "true",
                ["DevData:SharedSecret"] = "dev-integration-secret-value",
            });
        HttpClient client = factory.CreateClient();
        string userId = await RegisterUserIdAsync(client).ConfigureAwait(false);

        HttpResponseMessage res = await client
            .PostAsJsonAsync(new Uri("/api/dev/books/reset", UriKind.Relative), new { userId })
            .ConfigureAwait(false);

        _ = res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DevBook_WithRequiredSecret_ValidHeader_Succeeds()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        const string secret = "dev-integration-secret-value";
        using ExpenseTrackerApiFactory factory = new(
            host.ConnectionString,
            new Dictionary<string, string?>
            {
                ["DevData:RequireSharedSecret"] = "true",
                ["DevData:SharedSecret"] = secret,
            });
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Dev-Data-Secret", secret);
        string userId = await RegisterUserIdAsync(client).ConfigureAwait(false);

        HttpResponseMessage res = await client
            .PostAsJsonAsync(new Uri("/api/dev/books/reset", UriKind.Relative), new { userId })
            .ConfigureAwait(false);

        _ = res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<string> RegisterUserIdAsync(HttpClient client)
    {
        string email = $"book_{Guid.NewGuid():N}@test.local";
        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password = "Testpass123" })
            .ConfigureAwait(false);
        _ = reg.EnsureSuccessStatusCode();
        AuthResponseDto? body = await reg.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);
        return body!.UserId;
    }
}
