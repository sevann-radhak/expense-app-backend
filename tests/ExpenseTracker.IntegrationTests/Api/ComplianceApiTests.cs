using ExpenseTracker.IntegrationTests.Fixtures;
using ExpenseTracker.IntegrationTests.Support;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ExpenseTracker.IntegrationTests.Api;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Api")]
public sealed class ComplianceApiTests(IntegrationHostFixture host)
{
    [Fact]
    public async Task DeleteAccount_WithWrongPassword_ReturnsUnauthorized()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        const string email = "u1@test.local";
        const string password = "Password1!";
        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = reg.StatusCode.Should().Be(HttpStatusCode.Created);
        AuthResponseDto? auth = await reg.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        HttpResponseMessage response = await client
            .PostAsJsonAsync(new Uri("/api/compliance/me/delete-account", UriKind.Relative), new { password = "WrongPassword!" })
            .ConfigureAwait(false);

        _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAccount_WithCorrectPassword_RemovesUserAndBlocksFurtherLogin()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        const string email = "gone@test.local";
        const string password = "Password1!";
        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = reg.StatusCode.Should().Be(HttpStatusCode.Created);
        AuthResponseDto? auth = await reg.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        HttpResponseMessage deleteResponse = await client
            .PostAsJsonAsync(new Uri("/api/compliance/me/delete-account", UriKind.Relative), new { password })
            .ConfigureAwait(false);

        _ = deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        client.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage loginAgain = await client
            .PostAsJsonAsync(new Uri("/api/auth/login", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = loginAgain.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExportHint_ReturnsSyncBookPointer()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        const string email = "hint@test.local";
        const string password = "Password1!";
        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = reg.StatusCode.Should().Be(HttpStatusCode.Created);
        AuthResponseDto? auth = await reg.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        HttpResponseMessage response =
            await client.GetAsync(new Uri("/api/compliance/me/export-hint", UriKind.Relative)).ConfigureAwait(false);
        _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        _ = json.Should().Contain("/api/sync/book");
    }
}
