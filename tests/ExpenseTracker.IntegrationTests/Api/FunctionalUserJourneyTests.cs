using ExpenseTracker.IntegrationTests.Fixtures;
using ExpenseTracker.IntegrationTests.Support;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ExpenseTracker.IntegrationTests.Api;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Api")]
[Trait("Category", "E2E")]
public sealed class FunctionalUserJourneyTests(IntegrationHostFixture host)
{
    [Fact]
    public async Task Register_Login_UpdateDisplayName_VerifyOnMe()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string email = $"journey_{Guid.NewGuid():N}@test.local";
        const string password = "Testpass123";

        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = reg.EnsureSuccessStatusCode();

        HttpResponseMessage login = await client
            .PostAsJsonAsync(new Uri("/api/auth/login", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = login.EnsureSuccessStatusCode();
        AuthResponseDto? auth = await login.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);

        using HttpRequestMessage patch = new(HttpMethod.Patch, new Uri("/api/users/me", UriKind.Relative))
        {
            Content = JsonContent.Create(new { displayName = "Journey User" }),
        };
        patch.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        HttpResponseMessage patchRes = await client.SendAsync(patch).ConfigureAwait(false);
        _ = patchRes.StatusCode.Should().Be(HttpStatusCode.OK);

        using HttpRequestMessage meReq = new(HttpMethod.Get, new Uri("/api/users/me", UriKind.Relative));
        meReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        HttpResponseMessage me = await client.SendAsync(meReq).ConfigureAwait(false);
        UserProfileDto? profile = await me.Content.ReadFromJsonAsync<UserProfileDto>().ConfigureAwait(false);
        _ = profile!.DisplayName.Should().Be("Journey User");
    }
}
