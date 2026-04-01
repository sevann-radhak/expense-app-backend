using DbUp;
using DbUp.Engine;

namespace ExpenseTracker.Migrations;

/// <summary>
/// Applies versioned SQL scripts (embedded resources) to Azure SQL or SQL Server.
/// </summary>
public static class BookDatabaseMigrator
{
    public const string DefaultJournalTable = "schemaversions";

    /// <param name="connectionString">ADO.NET connection string (e.g. user secrets).</param>
    /// <param name="createDatabaseIfMissing">When true, creates the database from the connection string if it does not exist.</param>
    public static DatabaseUpgradeResult Apply(string connectionString, bool createDatabaseIfMissing = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        if (createDatabaseIfMissing)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);
        }

        return DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(BookDatabaseMigrator).Assembly)
            .JournalToSqlTable("dbo", DefaultJournalTable)
            .LogToConsole()
            .Build()
            .PerformUpgrade();
    }
}
