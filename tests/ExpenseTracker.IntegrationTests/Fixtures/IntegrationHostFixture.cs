using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.Infrastructure.Services;
using ExpenseTracker.IntegrationTests.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;
using System.Data.Common;
using Testcontainers.MsSql;

namespace ExpenseTracker.IntegrationTests.Fixtures;

public sealed class IntegrationHostFixture : IAsyncLifetime
{
    private MsSqlContainer? _container;
    private Respawner? _respawner;

    public string ConnectionString { get; private set; } = string.Empty;

    public ExpenseTrackerApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _container = new MsSqlBuilder()
            .WithPassword("Test_Password123!")
            .Build();
        await _container.StartAsync().ConfigureAwait(false);
        ConnectionString = NormalizeConnectionString(_container.GetConnectionString());

        Factory = new ExpenseTrackerApiFactory(ConnectionString);
        _ = Factory.CreateClient();

        await using DbConnection conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync().ConfigureAwait(false);
        _respawner = await Respawner.CreateAsync(
            conn,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                WithReseed = true,
                TablesToIgnore =
                [
                    new Table("dbo", "__EFMigrationsHistory"),
                    new Table("dbo", "roles"),
                    new Table("dbo", "role_claims"),
                ],
            }).ConfigureAwait(false);
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException("Fixture not initialized.");
        }

        await using DbConnection conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync().ConfigureAwait(false);
        await _respawner.ResetAsync(conn).ConfigureAwait(false);
        await ClearAllApplicationUsersAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Removes Identity users after Respawn. Book rows may survive Respawn in some FK orders; clear book data per user first.
    /// </summary>
    private async Task ClearAllApplicationUsersAsync()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        DevBookDataService devBook = scope.ServiceProvider.GetRequiredService<DevBookDataService>();
        List<string> userIds = await userManager.Users.Select(u => u.Id).ToListAsync().ConfigureAwait(false);
        foreach (string id in userIds)
        {
            await devBook.ResetUserBookAsync(id).ConfigureAwait(false);
        }

        List<ApplicationUser> users = await userManager.Users.AsTracking().ToListAsync().ConfigureAwait(false);
        foreach (ApplicationUser u in users)
        {
            _ = await userManager.DeleteAsync(u).ConfigureAwait(false);
        }
    }

    public async Task DisposeAsync()
    {
        Factory?.Dispose();
        if (_container is not null)
        {
            await _container.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static string NormalizeConnectionString(string raw)
    {
        SqlConnectionStringBuilder b = new(raw)
        {
            TrustServerCertificate = true,
            MultipleActiveResultSets = true,
        };
        return b.ConnectionString;
    }
}
