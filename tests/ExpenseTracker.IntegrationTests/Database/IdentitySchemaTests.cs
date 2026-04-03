using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace ExpenseTracker.IntegrationTests.Database;

[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Integration.Database")]
public sealed class IdentitySchemaTests(IntegrationHostFixture host)
{
    [Fact]
    public async Task AfterStartup_RoleSeeder_HasExpectedRoles()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        _ = host.Factory.CreateClient();

        using IServiceScope scope = host.Factory.Services.CreateScope();
        RoleManager<IdentityRole> roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        _ = (await roles.RoleExistsAsync(AppRoles.User).ConfigureAwait(false)).Should().BeTrue();
        _ = (await roles.RoleExistsAsync(AppRoles.Admin).ConfigureAwait(false)).Should().BeTrue();
        _ = (await roles.RoleExistsAsync(AppRoles.SuperAdmin).ConfigureAwait(false)).Should().BeTrue();
    }

    [Fact]
    public async Task Register_PersistsUser_InDatabase()
    {
        await host.ResetDatabaseAsync().ConfigureAwait(false);
        using HttpClient client = host.Factory.CreateClient();
        string email = $"db_{Guid.NewGuid():N}@test.local";

        HttpResponseMessage reg = await client
            .PostAsJsonAsync(
                new Uri("/api/auth/register", UriKind.Relative),
                new { email, password = "Testpass123" })
            .ConfigureAwait(false);
        _ = reg.EnsureSuccessStatusCode();

        using IServiceScope scope = host.Factory.Services.CreateScope();
        UserManager<ApplicationUser> users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        ApplicationUser? u = await users.FindByEmailAsync(email).ConfigureAwait(false);

        _ = u.Should().NotBeNull();
        _ = u!.Email.Should().Be(email);
    }
}
