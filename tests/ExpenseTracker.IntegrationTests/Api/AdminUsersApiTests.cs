using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.IntegrationTests.Fixtures;
using ExpenseTracker.IntegrationTests.Support;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ExpenseTracker.IntegrationTests.Api;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Api")]
public sealed class AdminUsersApiTests(IntegrationHostFixture host)
{
    [Fact]
    public async Task BootstrapSuperAdmin_SecondCall_ReturnsConflict()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();

        using HttpRequestMessage first = new(HttpMethod.Post, new Uri("/api/auth/bootstrap-superadmin", UriKind.Relative))
        {
            Content = JsonContent.Create(new { email = "super1@test.local", password = "SuperPass123!" }),
        };
        first.Headers.Add("X-Setup-Token", IntegrationTestConstants.BootstrapToken);
        HttpResponseMessage r1 = await client.SendAsync(first).ConfigureAwait(false);
        _ = r1.StatusCode.Should().Be(HttpStatusCode.OK);

        using HttpRequestMessage second = new(HttpMethod.Post, new Uri("/api/auth/bootstrap-superadmin", UriKind.Relative))
        {
            Content = JsonContent.Create(new { email = "super2@test.local", password = "SuperPass123!" }),
        };
        second.Headers.Add("X-Setup-Token", IntegrationTestConstants.BootstrapToken);
        HttpResponseMessage r2 = await client.SendAsync(second).ConfigureAwait(false);
        _ = r2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Bootstrap_WithoutToken_ReturnsUnauthorized()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();

        HttpResponseMessage r = await client
            .PostAsJsonAsync(new Uri("/api/auth/bootstrap-superadmin", UriKind.Relative), new { email = "x@test.local", password = "SuperPass123!" })
            .ConfigureAwait(false);

        _ = r.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListUsers_AsRegularUser_ReturnsForbidden()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string email = $"plain_{Guid.NewGuid():N}@test.local";
        const string password = "Testpass123";

        HttpResponseMessage reg = await client
            .PostAsJsonAsync(new Uri("/api/auth/register", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = reg.EnsureSuccessStatusCode();
        AuthResponseDto? auth = await reg.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);

        using HttpRequestMessage listReq = new(HttpMethod.Get, new Uri("/api/users?page=1&pageSize=10", UriKind.Relative));
        listReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        HttpResponseMessage list = await client.SendAsync(listReq).ConfigureAwait(false);
        _ = list.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_CannotCreateUserWithAdminRole_ReturnsForbidden()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string superToken = await BootstrapSuperAndLoginAsync(client).ConfigureAwait(false);

        string adminEmail = $"admin_{Guid.NewGuid():N}@test.local";
        using HttpRequestMessage createAdmin = new(HttpMethod.Post, new Uri("/api/users", UriKind.Relative))
        {
            Content = JsonContent.Create(new { email = adminEmail, password = "AdminPass123!", roles = new[] { AppRoles.Admin } }),
        };
        createAdmin.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superToken);
        HttpResponseMessage createRes = await client.SendAsync(createAdmin).ConfigureAwait(false);
        _ = createRes.EnsureSuccessStatusCode();

        HttpResponseMessage adminLogin = await client
            .PostAsJsonAsync(new Uri("/api/auth/login", UriKind.Relative), new { email = adminEmail, password = "AdminPass123!" })
            .ConfigureAwait(false);
        _ = adminLogin.EnsureSuccessStatusCode();
        AuthResponseDto? adminAuth = await adminLogin.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);

        string targetEmail = $"target_{Guid.NewGuid():N}@test.local";
        using HttpRequestMessage elevate = new(HttpMethod.Post, new Uri("/api/users", UriKind.Relative))
        {
            Content = JsonContent.Create(new { email = targetEmail, password = "UserPass123!", roles = new[] { AppRoles.Admin } }),
        };
        elevate.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth!.AccessToken);
        HttpResponseMessage forbidden = await client.SendAsync(elevate).ConfigureAwait(false);
        _ = forbidden.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuperAdmin_CreateUser_List_Get_PutRoles_Delete_Works()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        string superToken = await BootstrapSuperAndLoginAsync(client).ConfigureAwait(false);

        string newEmail = $"managed_{Guid.NewGuid():N}@test.local";
        using HttpRequestMessage create = new(HttpMethod.Post, new Uri("/api/users", UriKind.Relative))
        {
            Content = JsonContent.Create(new { email = newEmail, password = "ManagedPass123!", displayName = "Managed", roles = new[] { AppRoles.User } }),
        };
        create.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superToken);
        HttpResponseMessage created = await client.SendAsync(create).ConfigureAwait(false);
        _ = created.StatusCode.Should().Be(HttpStatusCode.Created);
        UserListItemResponseDto? createdBody = await created.Content.ReadFromJsonAsync<UserListItemResponseDto>().ConfigureAwait(false);
        _ = createdBody!.Email.Should().Be(newEmail);
        string userId = createdBody.Id;

        using HttpRequestMessage listReq = new(HttpMethod.Get, new Uri("/api/users?page=1&pageSize=50", UriKind.Relative));
        listReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superToken);
        HttpResponseMessage list = await client.SendAsync(listReq).ConfigureAwait(false);
        _ = list.EnsureSuccessStatusCode();
        JsonDocument listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync().ConfigureAwait(false));
        _ = listDoc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(2);

        using HttpRequestMessage getReq = new(HttpMethod.Get, new Uri($"/api/users/{userId}", UriKind.Relative));
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superToken);
        HttpResponseMessage get = await client.SendAsync(getReq).ConfigureAwait(false);
        _ = get.EnsureSuccessStatusCode();

        using HttpRequestMessage rolesReq = new(HttpMethod.Put, new Uri($"/api/users/{userId}/roles", UriKind.Relative))
        {
            Content = JsonContent.Create(new { roles = new[] { AppRoles.Admin, AppRoles.User } }),
        };
        rolesReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superToken);
        HttpResponseMessage rolesRes = await client.SendAsync(rolesReq).ConfigureAwait(false);
        _ = rolesRes.EnsureSuccessStatusCode();
        UserListItemResponseDto? afterRoles = await rolesRes.Content.ReadFromJsonAsync<UserListItemResponseDto>().ConfigureAwait(false);
        _ = afterRoles!.Roles.Should().Contain(AppRoles.Admin);

        using HttpRequestMessage deleteReq = new(HttpMethod.Delete, new Uri($"/api/users/{userId}", UriKind.Relative));
        deleteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superToken);
        HttpResponseMessage deleted = await client.SendAsync(deleteReq).ConfigureAwait(false);
        _ = deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SuperAdmin_CannotDeleteSelf_ReturnsBadRequest()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        HttpClient client = host.Factory.CreateClient();
        const string email = "superdel@test.local";
        const string password = "SuperPass123!";

        using HttpRequestMessage boot = new(HttpMethod.Post, new Uri("/api/auth/bootstrap-superadmin", UriKind.Relative))
        {
            Content = JsonContent.Create(new { email, password }),
        };
        boot.Headers.Add("X-Setup-Token", IntegrationTestConstants.BootstrapToken);
        _ = (await client.SendAsync(boot).ConfigureAwait(false)).EnsureSuccessStatusCode();

        HttpResponseMessage login = await client
            .PostAsJsonAsync(new Uri("/api/auth/login", UriKind.Relative), new { email, password })
            .ConfigureAwait(false);
        _ = login.EnsureSuccessStatusCode();
        AuthResponseDto? auth = await login.Content.ReadFromJsonAsync<AuthResponseDto>().ConfigureAwait(false);

        using HttpRequestMessage del = new(HttpMethod.Delete, new Uri($"/api/users/{auth!.UserId}", UriKind.Relative));
        del.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        HttpResponseMessage res = await client.SendAsync(del).ConfigureAwait(false);
        _ = res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<string> BootstrapSuperAndLoginAsync(HttpClient client)
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
