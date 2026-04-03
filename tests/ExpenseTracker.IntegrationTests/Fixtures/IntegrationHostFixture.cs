using ExpenseTracker.IntegrationTests.Support;
using Microsoft.Data.SqlClient;
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
