using ExpenseTracker.IntegrationTests.Fixtures;
using ExpenseTracker.IntegrationTests.Support;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ExpenseTracker.IntegrationTests.Api;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Api")]
public sealed class AuthApiTests(IntegrationHostFixture host)
{
    [Fact]
    public async Task Register_ThenLogin_ThenMe_ReturnsSameUser()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string email = $"user_{Guid.NewGuid():N}@test.local";
        const string password = "Testpass123";

        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = reg.StatusCode.Should().Be(HttpStatusCode.Created);
        AuthResponseDto? regBody = await reg.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);
        _ = regBody.Should().NotBeNull();
        _ = regBody!.UserId.Should().NotBeNullOrEmpty();
        _ = regBody.AccessToken.Should().NotBeNullOrEmpty();

        HttpResponseMessage login = await client
            .PostAsJsonAsync(new Uri("/api/auth/login", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = login.StatusCode.Should().Be(HttpStatusCode.OK);
        AuthResponseDto? loginBody = await login.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);
        _ = loginBody!.UserId.Should().Be(regBody.UserId);

        using HttpRequestMessage meReq = new(HttpMethod.Get, new Uri("/api/users/me", UriKind.Relative));
        meReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.AccessToken);
        HttpResponseMessage me = await client.SendAsync(meReq).ConfigureAwait(false);
        _ = me.StatusCode.Should().Be(HttpStatusCode.OK);
        UserProfileDto? profile = await me.Content.ReadFromJsonAsync<UserProfileDto>().ConfigureAwait(false);
        _ = profile!.Email.Should().Be(email);
    }

    [Fact]
    public async Task Logout_ThenMe_ReturnsUnauthorized()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string email = $"logout_{Guid.NewGuid():N}@test.local";
        const string password = "Testpass123";

        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = reg.EnsureSuccessStatusCode();
        AuthResponseDto? auth = await reg.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);

        using HttpRequestMessage logoutReq = new(HttpMethod.Post, new Uri("/api/auth/logout", UriKind.Relative));
        logoutReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        HttpResponseMessage logout = await client.SendAsync(logoutReq).ConfigureAwait(false);
        _ = logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using HttpRequestMessage meReq = new(HttpMethod.Get, new Uri("/api/users/me", UriKind.Relative));
        meReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        HttpResponseMessage me = await client.SendAsync(meReq).ConfigureAwait(false);
        _ = me.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UsersMe_WithoutToken_ReturnsUnauthorized()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();

        HttpResponseMessage me = await client.GetAsync(new Uri("/api/users/me", UriKind.Relative)).ConfigureAwait(false);

        _ = me.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
