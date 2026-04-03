using ExpenseTracker.IntegrationTests.Fixtures;
using ExpenseTracker.IntegrationTests.Support;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ExpenseTracker.IntegrationTests.Api;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Api")]
public sealed class BookSyncApiTests(IntegrationHostFixture host)
{
    [Fact]
    public async Task GetBook_Unauthenticated_Returns401()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        HttpResponseMessage res = await client.GetAsync(new Uri("/api/sync/book", UriKind.Relative)).ConfigureAwait(false);
        _ = res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBook_NewUser_ReturnsRevision0AndEmptySnapshot()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string token = await RegisterAndLoginAsync(client, $"sync_{Guid.NewGuid():N}@t.local").ConfigureAwait(false);

        using HttpRequestMessage req = new(HttpMethod.Get, new Uri("/api/sync/book", UriKind.Relative));
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage res = await client.SendAsync(req).ConfigureAwait(false);
        _ = res.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync().ConfigureAwait(false));
        _ = doc.RootElement.GetProperty("bookRevision").GetInt32().Should().Be(0);
        _ = doc.RootElement.GetProperty("schemaVersion").GetInt32().Should().Be(9);
        _ = doc.RootElement.GetProperty("categories").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task PutThenGet_RoundTripsMinimalBook()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string token = await RegisterAndLoginAsync(client, $"sync2_{Guid.NewGuid():N}@t.local").ConfigureAwait(false);

        string body = MinimalBookPutJson(0);
        using HttpRequestMessage put = new(HttpMethod.Put, new Uri("/api/sync/book", UriKind.Relative))
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        put.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage putRes = await client.SendAsync(put).ConfigureAwait(false);
        _ = putRes.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument putDoc = JsonDocument.Parse(await putRes.Content.ReadAsStringAsync().ConfigureAwait(false));
        _ = putDoc.RootElement.GetProperty("bookRevision").GetInt32().Should().Be(1);

        using HttpRequestMessage get = new(HttpMethod.Get, new Uri("/api/sync/book", UriKind.Relative));
        get.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage getRes = await client.SendAsync(get).ConfigureAwait(false);
        _ = getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument getDoc = JsonDocument.Parse(await getRes.Content.ReadAsStringAsync().ConfigureAwait(false));
        _ = getDoc.RootElement.GetProperty("bookRevision").GetInt32().Should().Be(1);
        _ = getDoc.RootElement.GetProperty("categories")[0].GetProperty("id").GetString().Should().Be("c1");
    }

    [Fact]
    public async Task Put_StaleExpectedRevision_Returns409WithCurrentRevision()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string token = await RegisterAndLoginAsync(client, $"sync3_{Guid.NewGuid():N}@t.local").ConfigureAwait(false);

        using HttpRequestMessage put1 = new(HttpMethod.Put, new Uri("/api/sync/book", UriKind.Relative))
        {
            Content = new StringContent(MinimalBookPutJson(0), Encoding.UTF8, "application/json"),
        };
        put1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _ = (await client.SendAsync(put1).ConfigureAwait(false)).EnsureSuccessStatusCode();

        using HttpRequestMessage put2 = new(HttpMethod.Put, new Uri("/api/sync/book", UriKind.Relative))
        {
            Content = new StringContent(MinimalBookPutJson(0), Encoding.UTF8, "application/json"),
        };
        put2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage conflict = await client.SendAsync(put2).ConfigureAwait(false);
        _ = conflict.StatusCode.Should().Be(HttpStatusCode.Conflict);
        JsonDocument err = JsonDocument.Parse(await conflict.Content.ReadAsStringAsync().ConfigureAwait(false));
        _ = err.RootElement.GetProperty("status").GetInt32().Should().Be(409);
        _ = err.RootElement.GetProperty("currentBookRevision").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task TwoUsers_DoNotSeeEachOthersBook()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string tokenA = await RegisterAndLoginAsync(client, $"a_{Guid.NewGuid():N}@t.local").ConfigureAwait(false);
        string tokenB = await RegisterAndLoginAsync(client, $"b_{Guid.NewGuid():N}@t.local").ConfigureAwait(false);

        using HttpRequestMessage putA = new(HttpMethod.Put, new Uri("/api/sync/book", UriKind.Relative))
        {
            Content = new StringContent(MinimalBookPutJson(0), Encoding.UTF8, "application/json"),
        };
        putA.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        _ = (await client.SendAsync(putA).ConfigureAwait(false)).EnsureSuccessStatusCode();

        using HttpRequestMessage getB = new(HttpMethod.Get, new Uri("/api/sync/book", UriKind.Relative));
        getB.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        HttpResponseMessage resB = await client.SendAsync(getB).ConfigureAwait(false);
        _ = resB.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument b = JsonDocument.Parse(await resB.Content.ReadAsStringAsync().ConfigureAwait(false));
        _ = b.RootElement.GetProperty("bookRevision").GetInt32().Should().Be(0);
        _ = b.RootElement.GetProperty("categories").GetArrayLength().Should().Be(0);
    }

    private static async Task<string> RegisterAndLoginAsync(HttpClient client, string email)
    {
        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password = "Testpass123" })
            .ConfigureAwait(false);
        _ = reg.EnsureSuccessStatusCode();
        AuthResponseDto? auth = await reg.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);
        return auth!.AccessToken;
    }

    private static string MinimalBookPutJson(int expectedRevision)
    {
        string exportedAt = DateTime.UtcNow.ToString("O");
        return $$"""
        {
          "expectedBookRevision": {{expectedRevision}},
          "schemaVersion": 9,
          "exportedAt": "{{exportedAt}}",
          "categories": [{"id":"c1","name":"C","sortOrder":0,"isActive":true}],
          "subcategories": [{"id":"s1","categoryId":"c1","name":"S","slug":"s","isSystemReserved":false,"sortOrder":0,"isActive":true}],
          "incomeCategories": [{"id":"ic1","name":"I","sortOrder":0,"isActive":true}],
          "incomeSubcategories": [{"id":"is1","categoryId":"ic1","name":"S","slug":"s","isSystemReserved":false,"sortOrder":0,"isActive":true}],
          "paymentInstruments": [],
          "expenseRecurringSeries": [],
          "expenses": [],
          "incomeEntries": [],
          "incomeRecurringSeries": [],
          "installmentPlans": [],
          "partialPayments": []
        }
        """;
    }
}
