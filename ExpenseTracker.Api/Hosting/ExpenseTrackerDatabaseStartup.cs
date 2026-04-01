using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Hosting;

public static class ExpenseTrackerDatabaseStartup
{
    public static async Task EnsureCreatedAndMigratedAsync(
        this WebApplication app,
        string? connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        await SqlServerDatabaseCreator.EnsureDatabaseExistsAsync(connectionString, cancellationToken)
            .ConfigureAwait(false);
        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        ExpenseTrackerDbContext? db = scope.ServiceProvider.GetService<ExpenseTrackerDbContext>();
        if (db is not null)
        {
            await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
