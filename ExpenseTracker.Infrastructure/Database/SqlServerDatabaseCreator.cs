using Microsoft.Data.SqlClient;

namespace ExpenseTracker.Infrastructure.Database;

public static class SqlServerDatabaseCreator
{
    /// <summary>Creates the database from the connection string if it does not exist (connects to <c>master</c>).</summary>
    public static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        SqlConnectionStringBuilder builder = new(connectionString);
        var database = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException(
                "Connection string must include Initial Catalog (database name) to auto-create the database.");
        }

        builder.InitialCatalog = "master";
        await using SqlConnection connection = new(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = connection.CreateCommand();
        var escapedName = database.Replace("'", "''");
        var bracketName = database.Replace("]", "]]");
        cmd.CommandText = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{escapedName}')
                EXEC(N'CREATE DATABASE [{bracketName}]');
            """;
        _ = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
