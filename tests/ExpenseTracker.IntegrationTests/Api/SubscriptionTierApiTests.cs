using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.IntegrationTests.Fixtures;
using ExpenseTracker.IntegrationTests.Support;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ExpenseTracker.IntegrationTests.Api;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Api")]
public sealed class SubscriptionTierApiTests(IntegrationHostFixture host)
{
    [Fact]
    public async Task Register_Me_IncludesBasicTier()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string email = $"tier_{Guid.NewGuid():N}@test.local";

        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password = "Testpass123" })
            .ConfigureAwait(false);
        _ = reg.EnsureSuccessStatusCode();
        AuthResponseDto? auth = await reg.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);

        using HttpRequestMessage meReq = new(HttpMethod.Get, new Uri("/api/users/me", UriKind.Relative));
        meReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        HttpResponseMessage me = await client.SendAsync(meReq).ConfigureAwait(false);
        UserProfileDto? profile = await me.Content.ReadFromJsonAsync<UserProfileDto>().ConfigureAwait(false);
        _ = profile!.SubscriptionTier.Should().Be(SubscriptionTierCodes.Basic);
        _ = profile.SubscriptionTierSource.Should().Be(SubscriptionTierSourceCodes.Default);
    }

    [Fact]
    public async Task SuperAdmin_PutUser_UpdatesSubscriptionTier()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string superToken = await BootstrapSuperTokenAsync(client).ConfigureAwait(false);

        string userEmail = $"sub_{Guid.NewGuid():N}@test.local";
        using HttpRequestMessage create = new(HttpMethod.Post, new Uri("/api/users", UriKind.Relative))
        {
            Content = JsonContent.Create(new { email = userEmail, password = "UserPass123!", roles = new[] { AppRoles.User } }),
        };
        create.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superToken);
        HttpResponseMessage created = await client.SendAsync(create).ConfigureAwait(false);
        _ = created.EnsureSuccessStatusCode();
        UserListItemResponseDto? createdBody = await created.Content.ReadFromJsonAsync<UserListItemResponseDto>().ConfigureAwait(false);
        string userId = createdBody!.Id;

        using HttpRequestMessage put = new(HttpMethod.Put, new Uri($"/api/users/{userId}", UriKind.Relative))
        {
            Content = JsonContent.Create(new { subscriptionTier = SubscriptionTierCodes.Pro }),
        };
        put.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superToken);
        HttpResponseMessage putRes = await client.SendAsync(put).ConfigureAwait(false);
        _ = putRes.EnsureSuccessStatusCode();
        UserListItemResponseDto? after = await putRes.Content.ReadFromJsonAsync<UserListItemResponseDto>().ConfigureAwait(false);
        _ = after!.SubscriptionTier.Should().Be(SubscriptionTierCodes.Pro);
        _ = after.SubscriptionTierSource.Should().Be(SubscriptionTierSourceCodes.Admin);
    }

    private static async Task<string> BootstrapSuperTokenAsync(HttpClient client)
    {
        using HttpRequestMessage boot = new(HttpMethod.Post, new Uri("/api/auth/bootstrap-superadmin", UriKind.Relative))
        {
            Content = JsonContent.Create(new { email = $"super_{Guid.NewGuid():N}@test.local", password = "SuperPass123!" }),
        };
        boot.Headers.Add("X-Setup-Token", IntegrationTestConstants.BootstrapToken);
        HttpResponseMessage bootRes = await client.SendAsync(boot).ConfigureAwait(false);
        _ = bootRes.EnsureSuccessStatusCode();
        JsonDocument doc = JsonDocument.Parse(await bootRes.Content.ReadAsStringAsync().ConfigureAwait(false));
        string email = doc.RootElement.GetProperty("email").GetString()!;
        HttpResponseMessage login = await client
            .PostAsJsonAsync(new Uri("/api/auth/login", UriKind.Relative), new { email, password = "SuperPass123!" })
            .ConfigureAwait(false);
        _ = login.EnsureSuccessStatusCode();
        AuthResponseDto? auth = await login.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);
        return auth!.AccessToken;
    }
}
